import axios from "axios";

// Base URL of your backend API
// export const API_BASE_URL = "https://localhost:44384/api/JitsiWebhook"; // Replace with your actual backend URL
// export const API_BASE_URL = "https://0a52-74-140-200-117.ngrok-free.app/api/JitsiWebhook"; // Replace with your actual backend URL
// export const API_BASE_URL = "https://6486-74-140-200-117.ngrok-free.app/api/JitsiWebhook"; // Replace with your actual backend URL
// export const API_BASE_URL = "https://5869-74-140-200-117.ngrok-free.app/api/JitsiWebhook";
export const API_BASE_URL = "https://6a7f-74-140-200-117.ngrok-free.app/api/JitsiWebhook";

export const getMeetingsByParticipant = async (email: string) => {
  try {
    const response = await axios.get(`${API_BASE_URL}/participant-meetings/${email}`);
    return response.data;
  } catch (error: any) {
    console.error("Error fetching meetings:", error);
    throw new Error(error.response?.data?.message || "Failed to fetch meetings");
  }
};