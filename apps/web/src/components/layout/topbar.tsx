"use client";

import { useRouter } from "next/navigation";
import { useMutation } from "@tanstack/react-query";
import { Moon, Sun, LogOut } from "lucide-react";
import { useTheme } from "next-themes";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useAuthStore } from "@/stores/auth-store";
import { authApi } from "@/lib/auth-api";
import { MobileSidebar } from "@/components/layout/sidebar";

function initials(fullName: string) {
  return fullName
    .split(" ")
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

export function Topbar() {
  const router = useRouter();
  const { resolvedTheme, setTheme } = useTheme();
  const user = useAuthStore((s) => s.user);
  const clearSession = useAuthStore((s) => s.clearSession);

  const logout = useMutation({
    mutationFn: authApi.logout,
    onSettled: () => {
      clearSession();
      router.push("/login");
    },
  });

  return (
    <header className="border-border flex h-14 shrink-0 items-center justify-between border-b px-4 md:px-6">
      <MobileSidebar />

      <div className="flex items-center gap-2">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
          aria-label="Cambiar tema"
        >
          <Sun className="size-4 scale-100 rotate-0 dark:scale-0 dark:-rotate-90 transition-transform" />
          <Moon className="absolute size-4 scale-0 rotate-90 dark:scale-100 dark:rotate-0 transition-transform" />
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger
            render={
              <Button variant="ghost" className="h-9 gap-2 px-2">
                <Avatar className="size-7">
                  <AvatarFallback className="text-xs">
                    {user ? initials(user.fullName) : "…"}
                  </AvatarFallback>
                </Avatar>
                <span className="hidden text-sm font-medium sm:inline">{user?.fullName}</span>
              </Button>
            }
          />
          <DropdownMenuContent align="end" className="w-56">
            <DropdownMenuGroup>
              <DropdownMenuLabel className="font-normal">
                <p className="text-sm font-medium">{user?.fullName}</p>
                <p className="text-muted-foreground text-xs">{user?.email}</p>
              </DropdownMenuLabel>
            </DropdownMenuGroup>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => logout.mutate()} disabled={logout.isPending}>
              <LogOut className="size-4" />
              Cerrar sesión
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
