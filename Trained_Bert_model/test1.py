import torch
from transformers import BertTokenizer, BertModel, AutoModelForSequenceClassification
import json
import traceback
import numpy as np
import os
from torch import nn
from flask import Flask, render_template, jsonify, request
from flask_cors import CORS
import re
from datetime import datetime
from sklearn.feature_extraction.text import TfidfVectorizer
from nltk.tokenize import sent_tokenize
import nltk

# Download required NLTK data
nltk.download('punkt', quiet=True)
nltk.download('averaged_perceptron_tagger', quiet=True)

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

class EnhancedMeetingAnalyzer:
    def __init__(self, model_path="trained_model"):
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
        self.tokenizer = BertTokenizer.from_pretrained('bert-base-uncased')
        self.vectorizer = TfidfVectorizer(max_features=10)
        self.load_model(model_path)
        
        # Enhanced keywords dictionary
        self.learning_keywords = {
            'assignment': 5,
            'deadline': 5,
            'quiz': 4,
            'exam': 5,
            'submission': 4,
            'project': 4,
            'presentation': 4,
            'paper': 4,
            'research': 4,
            'report': 4,
            'homework': 4,
            'study': 3,
            'review': 4,
            'analysis': 3,
            'discussion': 3,
            'meeting': 3,
            'feedback': 3,
            'proposal': 4,
            'methodology': 3,
            'overview': 3
        }

        # Role identification patterns
        self.speaker_patterns = {
            'instructor': ['teacher', 'professor', 'dr', 'mr', 'ms', 'mrs', 'instructor'],
            'student': ['student','Druvi','Thanoj','Geethika']
        }

    def load_model(self, path):
        try:
            with open(f"{path}/config.json", 'r') as f:
                config = json.load(f)
            self.model = MeetingAnalysisModel(n_classes=config['model_config']['n_classes'])
            self.model.load_state_dict(torch.load(f"{path}/model_state.pt", map_location=self.device))
            self.model.to(self.device)
            self.model.eval()
            print("Model loaded successfully!")
            return True
        except Exception as e:
            print(f"Error loading model: {str(e)}")
            return False

    def identify_speakers(self, conversation):
        """Identify speakers and their roles in the conversation"""
        speakers = {}
        for line in conversation.split('\n'):
            if ':' in line:
                speaker = line.split(':')[0].strip()
                speaker_lower = speaker.lower()  # Convert to lowercase for case-insensitive matching
                
                # Check if the speaker matches any pattern for instructor or student
                if any(p in speaker_lower for p in self.speaker_patterns['instructor']):
                    role = 'instructor'
                elif any(p in speaker_lower for p in self.speaker_patterns['student']):
                    role = 'student'
                else:
                    role = 'unknown'  # Default to unknown if not found

                speakers[speaker] = role
        return speakers

    def extract_key_points(self, conversation):
        """Extract key points from the conversation"""
        key_points = []
        sentences = sent_tokenize(conversation)
        
        important_markers = [
            'important', 'key', 'must', 'should', 'need to', 'remember',
            'don\'t forget', 'make sure', 'crucial', 'essential'
        ]
        
        for sentence in sentences:
            if any(marker in sentence.lower() for marker in important_markers):
                key_points.append(sentence)
                
        return key_points
    
    def analyze_conversation(self, conversation_text):
        try:
            if self.model is None:
                raise Exception("Model not loaded")

            # Process text in chunks
            chunks = [conversation_text[i:i + 450] for i in range(0, len(conversation_text), 450)]
            all_predictions = []
            
            for chunk in chunks:
                # Get tokenizer outputs
                tokenizer_output = self.tokenizer(
                    chunk,
                    max_length=450,
                    truncation=True,
                    padding=True,
                    return_tensors="pt"
                )
                
                # Explicitly select only input_ids and attention_mask
                input_ids = tokenizer_output['input_ids'].to(self.device)
                attention_mask = tokenizer_output['attention_mask'].to(self.device)

                # Forward pass with only required parameters
                with torch.no_grad():
                    outputs = self.model(input_ids=input_ids, attention_mask=attention_mask)
                    predictions = torch.sigmoid(outputs).cpu().numpy()[0]
                    all_predictions.append(predictions)

            # Combine predictions from all chunks
            final_predictions = np.mean(all_predictions, axis=0) if len(all_predictions) > 0 else np.zeros(4)

            # Extract information from conversation
            learning_items = []
            deadlines = []
            dates = []
            key_points = []

            for line in conversation_text.split('\n'):
                line = line.strip()
                if not line:
                    continue

                # Date extraction
                date_patterns = [
                    r'\d{2}-\d{2}-\d{4}',
                    r'(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]* \d{1,2}(?:st|nd|rd|th)?,? \d{4}',
                    r'next \w+',
                    r'this \w+',
                    r'in \d+ weeks?',
                    r'by (today|tomorrow|tonight)',
                    r'end of \w+'
                ]
                
                has_date = False
                for pattern in date_patterns:
                    if re.search(pattern, line, re.IGNORECASE):
                        dates.append(line)
                        has_date = True
                        break

                # Activity extraction
                for keyword in self.learning_keywords:
                    if keyword.lower() in line.lower():
                        learning_items.append({
                            'activity': line,
                            'type': keyword,
                            'priority': self._calculate_priority(line, keyword, has_date),
                            'has_deadline': has_date,
                            'speaker': line.split(':')[0].strip() if ':' in line else 'Unknown'
                        })
                        break

                # Key points extraction
                importance_markers = ['must', 'required', 'important', 'necessary', 'essential']
                if any(marker in line.lower() for marker in importance_markers):
                    key_points.append(line)

                # Deadline extraction
                deadline_keywords = ['due', 'deadline', 'by', 'submit', 'complete']
                if has_date and any(keyword in line.lower() for keyword in deadline_keywords):
                    deadlines.append(line)

            # Calculate priority
            priority_score = sum(item['priority'] for item in learning_items) / len(learning_items) if learning_items else 0
            priority_level = "High" if priority_score >= 4 else "Medium" if priority_score >= 2.5 else "Low"

            return {
                'learning_activities': learning_items,
                'deadlines': deadlines,
                'dates': dates,
                'key_points': key_points,
                'summary': {
                    'total_activities': len(learning_items),
                    'total_deadlines': len(deadlines),
                    'priority_level': priority_level,
                    'requires_immediate_attention': bool(final_predictions[1] > 0.5)
                }
            }

        except Exception as e:
            print(f"Error analyzing conversation: {str(e)}")
            traceback.print_exc()
            return None

    
    def _calculate_priority(self, text, keyword, has_deadline):
        """Calculate priority score for an activity"""
        base_priority = self.learning_keywords[keyword]
        priority = base_priority
        
        urgency_words = ['immediate', 'urgent', 'asap', 'today', 'tomorrow', 'quickly']
        if any(word in text.lower() for word in urgency_words):
            priority += 1
            
        if has_deadline:
            priority += 1
            
        return min(priority, 5)
    # Add these methods to the EnhancedMeetingAnalyzer class

    def _clean_text(self, text):
        """Clean individual text segments"""
        try:
            # Remove speaker prefixes
            if ':' in text:
                text = text.split(':', 1)[1]
            
            # Remove extra whitespace
            text = ' '.join(text.split())
            
            # Remove redundant punctuation
            text = re.sub(r'\.+', '.', text)
            text = re.sub(r'\s*\.\s*', '. ', text)
            
            # Capitalize first letter
            text = text.strip()
            if text:
                text = text[0].upper() + text[1:]
                
            return text
            
        except Exception as e:
            print(f"Error in _clean_text: {str(e)}")
            return text

    def _similar_content(self, text1, text2):
        """Check if two pieces of text are similar"""
        try:
            # Convert to sets of words for comparison
            words1 = set(text1.lower().split())
            words2 = set(text2.lower().split())
            
            # If either set is empty, return False
            if not words1 or not words2:
                return False
                
            # Calculate similarity ratio
            intersection = words1.intersection(words2)
            union = words1.union(words2)
            similarity = len(intersection) / len(union)
            
            # Return True if texts are more than 60% similar
            return similarity > 0.6
            
        except Exception as e:
            print(f"Error in _similar_content: {str(e)}")
            return False
    def _clean_summary(self, summary):
        """Clean up the generated summary"""
        try:
            if not summary:
                return summary
                
            # Remove speaker prefixes (e.g., "Name:")
            summary = re.sub(r'[A-Za-z]+\s*:', '', summary)
            
            # Fix multiple spaces
            summary = ' '.join(summary.split())
            
            # Fix multiple periods
            summary = re.sub(r'\.+', '.', summary)
            
            # Fix spacing around periods
            summary = re.sub(r'\s*\.\s*', '. ', summary)
            
            # Remove redundant information
            seen = set()
            cleaned_sentences = []
            for sentence in summary.split('. '):
                sentence = sentence.strip()
                if sentence and sentence.lower() not in seen:
                    seen.add(sentence.lower())
                    cleaned_sentences.append(sentence)
            
            # Join sentences and make sure first letter is capitalized
            final_summary = '. '.join(cleaned_sentences)
            if final_summary:
                final_summary = final_summary[0].upper() + final_summary[1:]
                if not final_summary.endswith('.'):
                    final_summary += '.'
                    
            return final_summary
            
        except Exception as e:
            print(f"Error in _clean_summary: {str(e)}")
            return summary


    def generate_summary(self, results):
        """Generate a comprehensive summary for any meeting conversation"""
        try:
            activities = results['learning_activities']
            
            # Enhanced patterns for better recognition
            patterns = {
                'meeting_start': [
                    'Today we will', 'let\'s discuss', 'today\'s session', 
                    'we\'ll be covering', 'let\'s start', 'today\'s topic'
                ],
                'topic_indicators': [
                    'focusing on', 'working on', 'my project is', 
                    'researching', 'studying', 'investigating',
                    'analyzing', 'developing', 'exploring'
                ],
                'feedback_indicators': [
                    'you should', 'try to', 'review','evaluate', 'consider', 
                    'might want to','suggest', 'advise', 'recommend',
                    'recommend', 'suggest', 'look into', 'make sure',
                    'don\'t forget', 'important to'
                ],
                'progress_indicators': [
                    'have completed', 'finished', 'working on',
                    'in progress', 'started', 'begun',
                    'gathered', 'collected', 'analyzed'
                ],
                'issue_indicators': [
                    'struggling with', 'having trouble', 'difficult',
                    'challenging', 'problem with', 'issue with',
                    'need help with', 'confused about'
                ],
                'decision_indicators': [
                    'decided to', 'agreed on', 'will proceed with',
                    'chosen to', 'planning to', 'going to',
                    'determined to', 'concluded to'
                ]
            }

            # Initialize structured components
            summary_parts = {
                'purpose': None,
                'topics': [],
                'progress': [],
                'issues': [],
                'feedback': [],
                'decisions': [],
                'deadlines': []
            }

            # Process each activity
            seen_content = set()
            for item in activities:
                text = item['activity'].lower()
                clean_text = self._clean_text(item['activity'])
                
                # Skip similar content
                if any(self._similar_content(text, seen) for seen in seen_content):
                    continue
                    
                # Identify meeting purpose
                if not summary_parts['purpose']:
                    if any(start in text for start in patterns['meeting_start']):
                        summary_parts['purpose'] = clean_text
                        seen_content.add(text)
                        continue

                # Identify topics and progress
                if any(indicator in text for indicator in patterns['topic_indicators']):
                    summary_parts['topics'].append(clean_text)
                    seen_content.add(text)
                
                # Identify issues
                if any(indicator in text for indicator in patterns['issue_indicators']):
                    summary_parts['issues'].append(clean_text)
                    seen_content.add(text)

                # Identify feedback
                if item['speaker'].lower().startswith(('teacher', 'professor', 'dr')):
                    if any(indicator in text for indicator in patterns['feedback_indicators']):
                        summary_parts['feedback'].append(clean_text)
                        seen_content.add(text)

                # Identify decisions
                if any(indicator in text for indicator in patterns['decision_indicators']):
                    summary_parts['decisions'].append(clean_text)
                    seen_content.add(text)

            # Add deadlines if any
            if results['deadlines']:
                summary_parts['deadlines'] = [self._clean_text(d) for d in results['deadlines']]

            # Build coherent summary
            final_parts = []
            
            # Add purpose
            if summary_parts['purpose']:
                final_parts.append(summary_parts['purpose'])
            
            if 'topics' in summary_parts and summary_parts['topics']:  # Check if 'topics' exists and is non-empty
                # Filter non-empty topics and assign the first three valid ones
                formatted_topics = [t for t in summary_parts['topics'][:3] if t]

                if formatted_topics:  # Proceed only if there are valid topics
                    topics_section = f"The discussion covered: {'; '.join(formatted_topics)}"
                    final_parts.append(topics_section)
                else:
                    # If no topics are found after filtering, you can handle that case here
                    final_parts.append("No specific topics were discussed.")
            else:
                # Handle the case where 'topics' is empty or not present
                final_parts.append("No topics were identified.")

            
            # Add issues if any
            if summary_parts['issues']:
                issues_text = "Challenges discussed included: " + "; ".join(summary_parts['issues'][:2])
                final_parts.append(issues_text)
            
            # Add feedback
            if summary_parts['feedback']:
                feedback_text = "Key suggestions provided: " + "; ".join(summary_parts['feedback'][:2])
                final_parts.append(feedback_text)
            
            # Add decisions
            if summary_parts['decisions']:
                decisions_text = "Decisions made: " + "; ".join(summary_parts['decisions'][:2])
                final_parts.append(decisions_text)
            
            # Add deadlines
            if summary_parts['deadlines']:
                deadlines_text = "Deadlines: " + "; ".join(summary_parts['deadlines'])
                final_parts.append(deadlines_text)

            # Combine and clean
            final_summary = " ".join(final_parts)
            return self._clean_summary(final_summary)

        except Exception as e:
            print(f"Error generating summary: {str(e)}")
            return "Proper summary can't be created"
          

app = Flask(__name__)
CORS(app)

@app.route('/')
def home():
    return render_template('index.html')

@app.route('/analyze', methods=['POST'])
def analyze():
    try:
        conversation = request.json['conversation']
        analyzer = EnhancedMeetingAnalyzer()
        results = analyzer.analyze_conversation(conversation)
            
        if results:
                summary = analyzer.generate_summary(results)
                return jsonify({
                    'summary': summary,
                    'actionItems': results['learning_activities'],
                    'deadlines': results['deadlines']
                })
                
        return jsonify({'error': 'Analysis failed'})
            
    except Exception as e:
            print(f"Error in analysis: {str(e)}")
            traceback.print_exc()
            return jsonify({'error': str(e)})

if __name__ == "__main__":
    app.run(debug=True)