import './App.css'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LandingPage from './LandingPage';
import MeetingsPage from './MeetingsPage'
import JitsiMeet from './JitsiMeet';
import MeetingDetailsPage from './MeetingDetailsPage';

function App() {
  return (
    <>
      <Router>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/meetings" element={<MeetingsPage />} />
        <Route path="/start-meeting" element={<JitsiMeet />} />
        <Route path="/meeting/:meetingId" element={<MeetingDetailsPage />} />
      </Routes>
    </Router>
    </>
  )
}

export default App
