import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import AppLayout from './components/layout/AppLayout';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import JobOfferDetail from './pages/JobOfferDetail';
import ActiveJobDetail from './pages/ActiveJobDetail';
import JobHistory from './pages/JobHistory';
import Earnings from './pages/Earnings';
import Profile from './pages/Profile';
import Achievements from './pages/Achievements';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const token = localStorage.getItem('partner_token');
  if (!token) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        
        <Route path="/" element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
          <Route index element={<Navigate to="/home" replace />} />
          <Route path="home" element={<Home />} />
          <Route path="jobs/offered/:id" element={<JobOfferDetail />} />
          <Route path="jobs/active/:id" element={<ActiveJobDetail />} />
          <Route path="jobs" element={<JobHistory />} />
          <Route path="earnings" element={<Earnings />} />
          <Route path="profile" element={<Profile />} />
          <Route path="achievements" element={<Achievements />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
