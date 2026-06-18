import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import OperatorLayout from './components/layout/OperatorLayout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Quotes from './pages/Quotes';
import QuoteReview from './pages/QuoteReview';
import Dispatch from './pages/Dispatch';
import DispatchAssign from './pages/DispatchAssign';
import Jobs from './pages/Jobs';
import JobDetail from './pages/JobDetail';
import Payouts from './pages/Payouts';
import Analytics from './pages/Analytics';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/" element={<OperatorLayout />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<Dashboard />} />
            <Route path="quotes" element={<Quotes />} />
            <Route path="quotes/:id/review" element={<QuoteReview />} />
            <Route path="dispatch" element={<Dispatch />} />
            <Route path="dispatch/:id/assign" element={<DispatchAssign />} />
            <Route path="jobs" element={<Jobs />} />
            <Route path="jobs/:id" element={<JobDetail />} />
            <Route path="payouts" element={<Payouts />} />
            <Route path="analytics" element={<Analytics />} />
          </Route>
        </Routes>
      </Router>
    </QueryClientProvider>
  );
}

export default App;