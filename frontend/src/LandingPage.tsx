import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './LandingPage.css';

const LandingPage = () => {
    const navigate = useNavigate();
    const [email, setEmail] = useState<string>("");
    const [showPopup, setShowPopup] = useState<boolean>(false);

    const handleLogin = () => {
        if (email.trim()) {
            localStorage.setItem("userEmail", email);
            setShowPopup(false);
        } else {
            alert("Please enter a valid email address.");
        }
    };

    return (
        <div className="landing-page" style={{textAlign:"center"}}>

            <div className='header'>
                {email ? <div>{email}</div> : <button onClick={() => setShowPopup(true)}>Login</button>}
                {showPopup && (
                    <div
                        style={{
                            position: "fixed",
                            top: "50%",
                            left: "50%",
                            transform: "translate(-50%, -50%)",
                            backgroundColor: "white",
                            padding: "20px",
                            boxShadow: "0 4px 8px rgba(0, 0, 0, 0.2)",
                            borderRadius: "8px",
                            zIndex: 1000,
                        }}
                    >
                        <h2>Enter Your Email</h2>
                        <input
                            type="email"
                            placeholder="Enter email address"
                            value={email}
                            onChange={(e) => {
                                setEmail(e.target.value);
                            }}
                            style={{ padding: "10px", marginBottom: "10px", width: "100%" }}
                        />
                        <button onClick={handleLogin} style={{ marginRight: "10px" }}>
                            Submit
                        </button>
                        <button onClick={() => setShowPopup(false)}>Cancel</button>
                    </div>
                )}
            </div>

            <h1>Welcome to Our App</h1>
            <div className="buttons">
                <button onClick={() => navigate('/start-meeting')}>Start a Meeting</button>
                <button onClick={() => navigate('/meetings')}>View Previous Meetings</button>
            </div>
        </div>
    );
};

export default LandingPage;