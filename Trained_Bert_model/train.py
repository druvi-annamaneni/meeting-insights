import torch
from torch import nn
from torch.utils.data import Dataset, DataLoader
from transformers import BertModel, BertTokenizer
from sklearn.model_selection import train_test_split
import pandas as pd
import numpy as np
import json
import os

class MeetingDataset(Dataset):
    def __init__(self, texts, labels, tokenizer, max_length=450):
        self.texts = texts
        self.labels = labels
        self.tokenizer = tokenizer
        self.max_length = max_length

    def __len__(self):
        return len(self.texts)

    def __getitem__(self, idx):
        text = str(self.texts[idx])
        encoding = self.tokenizer(
            text,
            add_special_tokens=True,
            max_length=self.max_length,
            padding='max_length',
            truncation=True,
            return_tensors='pt'
        )

        return {
            'input_ids': encoding['input_ids'].flatten(),
            'attention_mask': encoding['attention_mask'].flatten(),
            'labels': torch.FloatTensor(self.labels[idx])
        }

class MeetingAnalysisModel(nn.Module):
    def __init__(self, n_classes):
        super().__init__()
        self.bert = BertModel.from_pretrained('bert-base-uncased')
        self.dropout = nn.Dropout(0.3)
        self.classifier = nn.Linear(768, n_classes)

    def forward(self, input_ids, attention_mask):
        outputs = self.bert(
            input_ids=input_ids,
            attention_mask=attention_mask,
            return_dict=True
        )
        pooled_output = outputs.last_hidden_state[:, 0, :]
        dropout_output = self.dropout(pooled_output)
        return self.classifier(dropout_output)

def prepare_data(df):
    """Prepare the data for training"""
    print("Starting data preparation...")
    
    df['full_text'] = df.apply(lambda row: f"{row['Speaker']} ({row['Context']}): {row['Text']}", axis=1)
    conversations = df.groupby('Context')['full_text'].agg(list).reset_index()
    conversations['conversation'] = conversations['full_text'].apply(lambda x: ' '.join(x))
    
    features = []
    labels = []
    
    for _, row in conversations.iterrows():
        features.append(row['conversation'])
        
        context_df = df[df['Context'] == row['Context']]
        has_date = not all(pd.isna(context_df['Dates']))
        has_action = not all(pd.isna(context_df['Actionable Words']))
        
        labels.append([
            float(has_date),
            float(has_action),
            float(1.0),  # Placeholder for priority
            float(0.5)   # Placeholder for sentiment
        ])
    
    print(f"Processed {len(features)} conversations")
    return features, np.array(labels)

def train_model(features, labels, save_path="trained_model", epochs=3, batch_size=8):
    device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    print(f"Using device: {device}")
    
    # Initialize tokenizer and model
    tokenizer = BertTokenizer.from_pretrained('bert-base-uncased')
    model = MeetingAnalysisModel(n_classes=len(labels[0]))
    model.to(device)
    
    # Prepare data loader
    dataset = MeetingDataset(features, labels, tokenizer)
    train_loader = DataLoader(dataset, batch_size=batch_size, shuffle=True)
    
    # Training setup
    optimizer = torch.optim.AdamW(model.parameters(), lr=2e-5)
    criterion = nn.MSELoss()
    
    # Training loop
    for epoch in range(epochs):
        model.train()
        total_loss = 0
        
        for batch_idx, batch in enumerate(train_loader):
            optimizer.zero_grad()
            
            input_ids = batch['input_ids'].to(device)
            attention_mask = batch['attention_mask'].to(device)
            labels = batch['labels'].to(device)
            
            outputs = model(input_ids, attention_mask)
            loss = criterion(outputs, labels)
            
            loss.backward()
            optimizer.step()
            
            total_loss += loss.item()
            
            if batch_idx % 5 == 0:
                print(f"Epoch {epoch + 1}, Batch {batch_idx}, Loss: {loss.item():.4f}")
        
        avg_loss = total_loss / len(train_loader)
        print(f"Epoch {epoch + 1} completed. Average loss: {avg_loss:.4f}")
    
    # Save the model
    os.makedirs(save_path, exist_ok=True)
    torch.save(model.state_dict(), f"{save_path}/model_state.pt")
    tokenizer.save_pretrained(f"{save_path}/tokenizer")
    
    # Save configuration
    config = {
        'model_config': {
            'n_classes': len(labels[0])
        }
    }
    with open(f"{save_path}/config.json", 'w') as f:
        json.dump(config, f)
    
    print(f"Model saved to {save_path}")
    return model, tokenizer

def main():
    try:
        print("Loading data...")
        df = pd.read_csv('randomized_names_conversation_dataset.csv')
        
        print("Preparing data...")
        features, labels = prepare_data(df)
        
        print("Starting model training...")
        model, tokenizer = train_model(features, labels)
        
        print("Training completed!")
        
    except Exception as e:
        print(f"Error in training: {str(e)}")

if __name__ == "__main__":
    main()