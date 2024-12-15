import React from 'react';
import { JaaSMeeting } from '@jitsi/react-sdk';

const Meeting: React.FC = () => {
  const roomName = "DC Lecture"; 
const jwt = "eyJraWQiOiJ2cGFhcy1tYWdpYy1jb29raWUtOWJlY2YxMWQyODc4NDYzMTk2MDlkOWQ4ZThhOWQ2YzQvNDAzYWVmLVNBTVBMRV9BUFAiLCJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiJqaXRzaSIsImlzcyI6ImNoYXQiLCJpYXQiOjE3MzM4NjI2MDEsImV4cCI6MTczMzg2OTgwMSwibmJmIjoxNzMzODYyNTk2LCJzdWIiOiJ2cGFhcy1tYWdpYy1jb29raWUtOWJlY2YxMWQyODc4NDYzMTk2MDlkOWQ4ZThhOWQ2YzQiLCJjb250ZXh0Ijp7ImZlYXR1cmVzIjp7ImxpdmVzdHJlYW1pbmciOmZhbHNlLCJvdXRib3VuZC1jYWxsIjpmYWxzZSwic2lwLW91dGJvdW5kLWNhbGwiOmZhbHNlLCJ0cmFuc2NyaXB0aW9uIjp0cnVlLCJyZWNvcmRpbmciOmZhbHNlfSwidXNlciI6eyJoaWRkZW4tZnJvbS1yZWNvcmRlciI6ZmFsc2UsIm1vZGVyYXRvciI6dHJ1ZSwibmFtZSI6ImRydXZpYW5uYW1hbmVuaSIsImlkIjoiZ29vZ2xlLW9hdXRoMnwxMDA0MzkyMjkyMjAxOTUxODcyNTIiLCJhdmF0YXIiOiIiLCJlbWFpbCI6ImRydXZpYW5uYW1hbmVuaUBnbWFpbC5jb20ifX0sInJvb20iOiIqIn0.aFHWJ-xDTDic_buHgUxYkENub-8u9P1myWzk4YhnUDQ4FvC4xPS-16WzpwJd4584kwt4o1TcweXJLbT9agnctLXPgTzyugygOR7vyqIoyezXG2fcMCR_t9tsCyuXxeLetpxWXPndJo6FfA2xl32uYwfJN9vkVUGZylKnGoupt9s123YzevXPpabvPINnru1IfkVlqXOHa5HRefh-JRTRS5YUfgDf-bLy6A9w3JkR4p355EKiwl6CMGaLm8gcMUIKpicofXquIgkK0P_gKSWeFTpbefo6yLX3BDygfcMxQRpSoK0_9stmWxa_TMHgVja9ED0Omg8gukQ8GETUZixpxA";
const appId = "vpaas-magic-cookie-9becf11d287846319609d9d8e8a9d6c4";

  return (
    <div>
        <h2>Start your meeting from here</h2>
        <div className="jitsimeet-container">
        <JaaSMeeting
            appId = { appId }
            jwt = { jwt }
            roomName = { roomName }
        />
        </div>
    </div>
  );
};

export default Meeting;