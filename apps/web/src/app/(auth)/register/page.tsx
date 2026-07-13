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
import { registerSchema, type RegisterFormValues } from "@/lib/auth-schemas";
import { authApi } from "@/lib/auth-api";
import { useAuthStore } from "@/stores/auth-store";
import { ApiError } from "@/lib/api-client";

export default function RegisterPage() {
  const router = useRouter();
  const setSession = useAuthStore((s) => s.setSession);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) });

  const mutation = useMutation({
    mutationFn: authApi.register,
    onSuccess: (data) => {
      setSession(data.accessToken, data.user);
      router.push("/dashboard");
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors?.length) {
        error.problem.errors.forEach((e) => toast.error(e.error));
        return;
      }
      const message = error instanceof ApiError ? error.problem.title : "No se pudo crear la cuenta.";
      toast.error(message);
    },
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-xl">Crea tu empresa</CardTitle>
        <CardDescription>Registra tu empresa y tu cuenta de Owner en NovaERP.</CardDescription>
      </CardHeader>
      <CardContent>
        <form
          className="grid gap-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            id="companyName"
            label="Nombre de la empresa"
            autoComplete="organization"
            error={errors.companyName?.message}
            {...register("companyName")}
          />
          <FormField
            id="fullName"
            label="Tu nombre completo"
            autoComplete="name"
            error={errors.fullName?.message}
            {...register("fullName")}
          />
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
            autoComplete="new-password"
            error={errors.password?.message}
            {...register("password")}
          />
          <Button type="submit" disabled={mutation.isPending} className="mt-1">
            {mutation.isPending ? "Creando…" : "Crear cuenta"}
          </Button>
        </form>
        <p className="text-muted-foreground mt-6 text-center text-sm">
          ¿Ya tienes cuenta?{" "}
          <Link href="/login" className="text-foreground font-medium underline underline-offset-4">
            Inicia sesión
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}
