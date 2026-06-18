import { Link, Outlet, useLocation } from 'react-router-dom';
import { Home, ClipboardList, Wallet, User, Trophy } from 'lucide-react';
import clsx from 'clsx';

export default function AppLayout() {
  const location = useLocation();

  const navItems = [
    { icon: Home, label: 'Home', path: '/home' },
    { icon: ClipboardList, label: 'Jobs', path: '/jobs' },
    { icon: Wallet, label: 'Earnings', path: '/earnings' },
    { icon: Trophy, label: 'Rewards', path: '/achievements' },
    { icon: User, label: 'Profile', path: '/profile' },
  ];

  return (
    <div className="flex flex-col h-screen bg-slate-50">
      <main className="flex-1 overflow-y-auto pb-20">
        <Outlet />
      </main>

      <nav className="fixed bottom-0 w-full bg-white border-t border-slate-200 pb-safe">
        <div className="flex justify-around items-center h-16">
          {navItems.map((item) => {
            const isActive = location.pathname.startsWith(item.path);
            const Icon = item.icon;
            
            return (
              <Link
                key={item.path}
                to={item.path}
                className={clsx(
                  "flex flex-col items-center justify-center w-full h-full space-y-1 transition-colors",
                  isActive ? "text-primary" : "text-slate-500 hover:text-slate-900"
                )}
              >
                <Icon size={24} className={isActive ? "fill-primary/20" : ""} />
                <span className="text-[10px] font-medium">{item.label}</span>
              </Link>
            );
          })}
        </div>
      </nav>
    </div>
  );
}
