"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  Package,
  Contact,
  ShoppingCart,
  Receipt,
  Wallet,
  ShoppingBag,
  Users,
  FileBarChart,
  Settings,
} from "lucide-react";
import { cn } from "@/lib/utils";

const NAV_ITEMS = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/dashboard/inventario", label: "Inventario", icon: Package },
  { href: "/dashboard/contactos", label: "Contactos", icon: Contact },
  { href: "/dashboard/ventas", label: "Ventas", icon: ShoppingCart },
  { href: "/dashboard/facturacion", label: "Facturación", icon: Receipt },
  { href: "/dashboard/caja", label: "Caja", icon: Wallet },
  { href: "/dashboard/compras", label: "Compras", icon: ShoppingBag },
  { href: "/dashboard/usuarios", label: "Usuarios", icon: Users },
  { href: "/dashboard/reportes", label: "Reportes", icon: FileBarChart },
] as const;

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="border-border bg-sidebar hidden w-60 shrink-0 flex-col border-r px-3 py-4 md:flex">
      <div className="px-3 py-2">
        <span className="text-lg font-semibold tracking-tight">NovaERP</span>
      </div>

      <nav className="mt-4 flex flex-1 flex-col gap-0.5">
        {NAV_ITEMS.map((item) => {
          const isActive =
            item.href === "/dashboard" ? pathname === item.href : pathname.startsWith(item.href);

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                isActive
                  ? "bg-sidebar-accent text-sidebar-accent-foreground"
                  : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground",
              )}
            >
              <item.icon className="size-4" strokeWidth={2} />
              {item.label}
            </Link>
          );
        })}
      </nav>

      <Link
        href="/dashboard/configuracion"
        className="text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors"
      >
        <Settings className="size-4" strokeWidth={2} />
        Configuración
      </Link>
    </aside>
  );
}
