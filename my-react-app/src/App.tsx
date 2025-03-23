import './App.css'
import { BrowserRouter, Route, Routes } from "react-router-dom";
import NewsList from "./pages/NewsList";
import NewsDetails from "./pages/NewsDetails";

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* інші маршрути */}
                <Route path="/" element={<NewsList />} />
                <Route path="/news/:id" element={<NewsDetails />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;

