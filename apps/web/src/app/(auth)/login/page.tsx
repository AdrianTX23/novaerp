"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FormField } from "@/components/auth/form-field";
import { loginSchema, type LoginFormValues } from "@/lib/auth-schemas";
import { authApi } from "@/lib/auth-api";
import { useAuthStore } from "@/stores/auth-store";
import { ApiError } from "@/lib/api-client";

export default function LoginPage() {
  const router = useRouter();
  const setSession = useAuthStore((s) => s.setSession);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const mutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: (data) => {
      setSession(data.accessToken, data.user);
      router.push("/dashboard");
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo iniciar sesión.";
      toast.error(message);
    },
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-xl">Inicia sesión</CardTitle>
        <CardDescription>Entra a tu cuenta de NovaERP.</CardDescription>
      </CardHeader>
      <CardContent>
        <form
          className="grid gap-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            id="email"
            label="Email"
            type="email"
            autoComplete="email"
            error={errors.email?.message}
            {...register("email")}
          />
          <FormField
            id="password"
            label="Contraseña"
            type="password"
            autoComplete="current-password"
            error={errors.password?.message}
            {...register("password")}
          />
          <Button type="submit" disabled={mutation.isPending} className="mt-1">
            {mutation.isPending ? "Entrando…" : "Entrar"}
          </Button>
        </form>
        <p className="text-muted-foreground mt-6 text-center text-sm">
          ¿No tienes cuenta?{" "}
          <Link href="/register" className="text-foreground font-medium underline underline-offset-4">
            Crea tu empresa
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}
