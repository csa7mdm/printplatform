import { Link, Outlet, useLocation } from 'react-router-dom';
import { LayoutDashboard, FileText, Send, Briefcase, DollarSign, BarChart2, LogOut } from 'lucide-react';

const navigation = [
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { name: 'Quotes Queue', href: '/quotes', icon: FileText },
  { name: 'Dispatch', href: '/dispatch', icon: Send },
  { name: 'Active Jobs', href: '/jobs', icon: Briefcase },
  { name: 'Payouts', href: '/payouts', icon: DollarSign },
  { name: 'Analytics', href: '/analytics', icon: BarChart2 },
];

export default function OperatorLayout() {
  const location = useLocation();

  const handleLogout = () => {
    localStorage.removeItem('operator_token');
    window.location.href = '/login';
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <div className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="h-16 flex items-center px-6 border-b border-gray-200">
          <span className="text-xl font-bold text-primary-600">PrintPlatform Ops</span>
        </div>
        <div className="flex-1 py-4 flex flex-col gap-1 px-3">
          {navigation.map((item) => {
            const isActive = location.pathname.startsWith(item.href);
            return (
              <Link
                key={item.name}
                to={item.href}
                className={`flex items-center px-3 py-2 rounded-md text-sm font-medium ${
                  isActive
                    ? 'bg-primary-50 text-primary-700'
                    : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <item.icon className={`mr-3 h-5 w-5 ${isActive ? 'text-primary-600' : 'text-gray-400'}`} />
                {item.name}
              </Link>
            );
          })}
        </div>
        <div className="p-4 border-t border-gray-200">
          <button
            onClick={handleLogout}
            className="flex items-center w-full px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-md"
          >
            <LogOut className="mr-3 h-5 w-5 text-gray-400" />
            Sign Out
          </button>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6">
          <h1 className="text-lg font-medium text-gray-900">
            {navigation.find(n => location.pathname.startsWith(n.href))?.name || 'Overview'}
          </h1>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-500">Welcome, Operator</span>
          </div>
        </header>
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}