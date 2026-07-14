"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Target, TrendingUp, Trophy, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { crmApi } from "@/lib/crm-api";
import { ApiError } from "@/lib/api-client";
import type { OpportunityDto, OpportunityStageName } from "@/lib/types";
import { formatMoney, cn } from "@/lib/utils";
import { BOARD_STAGES, STAGE_LABEL, STAGE_VALUE, isClosedStage } from "@/components/crm/stage-meta";
import { CreateOpportunityDialog } from "@/components/crm/create-opportunity-dialog";

export function CrmBoard() {
  const queryClient = useQueryClient();
  const oppsQuery = useQuery({ queryKey: ["crm", "opportunities"], queryFn: crmApi.opportunities, retry: false });
  const pipelineQuery = useQuery({ queryKey: ["crm", "pipeline"], queryFn: crmApi.pipeline, retry: false });

  const forbidden =
    (oppsQuery.error instanceof ApiError && oppsQuery.error.status === 403) ||
    (pipelineQuery.error instanceof ApiError && pipelineQuery.error.status === 403);

  const move = useMutation({
    mutationFn: (vars: { id: string; stage: number }) => crmApi.move(vars.id, vars.stage),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["crm"] });
      toast.success("Oportunidad movida.");
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo mover la oportunidad.";
      toast.error(message);
    },
  });

  if (forbidden) {
    return (
      <div className="text-muted-foreground rounded-xl border border-dashed p-8 text-center text-sm">
        Tu rol no tiene acceso al CRM.
      </div>
    );
  }

  const summary = pipelineQuery.data;
  const opps = oppsQuery.data ?? [];
  const byStage = (stage: OpportunityStageName) => opps.filter((o) => o.stage === stage);
  const stageTotal = (stage: OpportunityStageName) =>
    summary?.byStage.find((s) => s.stage === stage) ?? { count: 0, value: 0 };

  return (
    <div className="grid gap-6">
      <div className="flex items-center justify-between">
        <div className="grid flex-1 gap-4 sm:grid-cols-3">
          <SummaryCard label="Pipeline abierto" value={summary ? formatMoney(summary.openValue) : "…"} icon={TrendingUp} />
          <SummaryCard label="Oportunidades abiertas" value={summary ? String(summary.openCount) : "…"} icon={Target} />
          <SummaryCard
            label="Ganado del mes"
            value={summary ? formatMoney(summary.wonThisMonth) : "…"}
            icon={Trophy}
            accent
          />
        </div>
      </div>

      <div className="flex items-center justify-between">
        <h2 className="font-medium">Pipeline</h2>
        <CreateOpportunityDialog />
      </div>

      {oppsQuery.isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : (
        <div className="flex gap-3 overflow-x-auto pb-2">
          {BOARD_STAGES.map((stage) => {
            const total = stageTotal(stage);
            const closed = isClosedStage(stage);
            return (
              <div key={stage} className="bg-muted/40 flex w-64 shrink-0 flex-col gap-2 rounded-xl p-2.5">
                <div className="flex items-center justify-between px-1">
                  <span className="text-sm font-medium">{STAGE_LABEL[stage]}</span>
                  <span className="text-muted-foreground text-xs tabular-nums">{total.count}</span>
                </div>
                <span className="text-muted-foreground px-1 text-xs tabular-nums">{formatMoney(total.value)}</span>

                <div className="grid gap-2">
                  {byStage(stage).map((o) => (
                    <OpportunityCard key={o.id} opp={o} closed={closed} onMove={move.mutate} busy={move.isPending} />
                  ))}
                  {byStage(stage).length === 0 && (
                    <p className="text-muted-foreground/60 px-1 py-4 text-center text-xs">Sin oportunidades</p>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

function SummaryCard({
  label,
  value,
  icon: Icon,
  accent,
}: {
  label: string;
  value: string;
  icon: typeof Target;
  accent?: boolean;
}) {
  return (
    <div className="bg-card flex items-center justify-between rounded-xl border p-4">
      <div>
        <div className="text-muted-foreground text-xs font-medium tracking-wide uppercase">{label}</div>
        <div
          className={cn(
            "mt-1 text-xl font-semibold tabular-nums",
            accent && "text-emerald-600 dark:text-emerald-500",
          )}
        >
          {value}
        </div>
      </div>
      <span className="bg-muted text-muted-foreground flex size-9 items-center justify-center rounded-lg">
        <Icon className="size-4" />
      </span>
    </div>
  );
}

function OpportunityCard({
  opp,
  closed,
  onMove,
  busy,
}: {
  opp: OpportunityDto;
  closed: boolean;
  onMove: (vars: { id: string; stage: number }) => void;
  busy: boolean;
}) {
  const targets = BOARD_STAGES.filter((s) => s !== opp.stage);
  return (
    <div className="bg-card rounded-lg border p-3">
      <p className="text-sm font-medium">{opp.title}</p>
      <p className="text-muted-foreground text-xs">{opp.customerName}</p>
      <div className="mt-2 flex items-center justify-between">
        <span className="text-sm font-semibold tabular-nums">{formatMoney(opp.estimatedValue)}</span>
        {!closed && (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={
                <Button variant="ghost" size="sm" disabled={busy} className="h-6 gap-1 px-1.5 text-xs">
                  Mover <ChevronDown className="size-3" />
                </Button>
              }
            />
            <DropdownMenuContent align="end">
              {targets.map((s) => (
                <DropdownMenuItem key={s} onClick={() => onMove({ id: opp.id, stage: STAGE_VALUE[s] })}>
                  {STAGE_LABEL[s]}
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </div>
  );
}
