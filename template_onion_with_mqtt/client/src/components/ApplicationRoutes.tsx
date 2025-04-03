import {BrowserRouter, Route, Routes} from "react-router";
import AdminDashboard from "./Dashboard.tsx";
import useInitializeData from "../hooks/useInitializeData.tsx";
import useSubscribeToTopics from "../hooks/UseSubscribeToTopics.tsx";

export default function ApplicationRoutes() {
    
    useInitializeData();
    useSubscribeToTopics();
    
    return(<>
        
        <BrowserRouter>
            <Routes>
                <Route element={<AdminDashboard />}  path='/' />
            </Routes>
        </BrowserRouter>
    
    </>)
}