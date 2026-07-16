"use client";

import { useState } from "react";
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
  BookOpen,
  Target,
  Users,
  FileBarChart,
  Settings,
  History,
  Menu,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { useAuthStore } from "@/stores/auth-store";

const NAV_ITEMS = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/dashboard/inventario", label: "Inventario", icon: Package },
  { href: "/dashboard/contactos", label: "Contactos", icon: Contact },
  { href: "/dashboard/crm", label: "CRM", icon: Target },
  { href: "/dashboard/ventas", label: "Ventas", icon: ShoppingCart },
  { href: "/dashboard/facturacion", label: "Facturación", icon: Receipt },
  { href: "/dashboard/caja", label: "Caja", icon: Wallet },
  { href: "/dashboard/contabilidad", label: "Contabilidad", icon: BookOpen },
  { href: "/dashboard/compras", label: "Compras", icon: ShoppingBag },
  { href: "/dashboard/usuarios", label: "Usuarios", icon: Users },
  { href: "/dashboard/reportes", label: "Reportes", icon: FileBarChart },
] as const;

function NavLinks({ onNavigate }: { onNavigate?: () => void }) {
  const pathname = usePathname();
  const canManage = useAuthStore((s) => s.user?.permissions.includes("users.manage") ?? false);

  return (
    <>
      <nav className="mt-4 flex flex-1 flex-col gap-0.5">
        {NAV_ITEMS.map((item) => {
          const isActive =
            item.href === "/dashboard" ? pathname === item.href : pathname.startsWith(item.href);

          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={onNavigate}
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

      {canManage && (
        <Link
          href="/dashboard/auditoria"
          onClick={onNavigate}
          className="text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors"
        >
          <History className="size-4" strokeWidth={2} />
          Auditoría
        </Link>
      )}

      <Link
        href="/dashboard/configuracion"
        onClick={onNavigate}
        className="text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors"
      >
        <Settings className="size-4" strokeWidth={2} />
        Configuración
      </Link>
    </>
  );
}

export function Sidebar() {
  return (
    <aside className="border-border bg-sidebar hidden w-60 shrink-0 flex-col border-r px-3 py-4 md:flex">
      <div className="px-3 py-2">
        <span className="text-lg font-semibold tracking-tight">NovaERP</span>
      </div>
      <NavLinks />
    </aside>
  );
}

export function MobileSidebar() {
  const [open, setOpen] = useState(false);

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger
        render={
          <Button variant="ghost" size="icon" className="md:hidden" aria-label="Abrir menú">
            <Menu className="size-5" />
          </Button>
        }
      />
      <SheetContent side="left" className="flex flex-col px-3 py-4">
        <SheetHeader className="px-3 py-2">
          <SheetTitle>NovaERP</SheetTitle>
        </SheetHeader>
        <NavLinks onNavigate={() => setOpen(false)} />
      </SheetContent>
    </Sheet>
  );
}
