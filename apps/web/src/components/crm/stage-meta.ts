import { OpportunityStage, type OpportunityStageName } from "@/lib/types";

export const STAGE_LABEL: Record<OpportunityStageName, string> = {
  New: "Nuevo",
  Qualified: "Calificado",
  Proposal: "Propuesta",
  Won: "Ganado",
  Lost: "Perdido",
};

/** Columnas del tablero, en orden. */
export const BOARD_STAGES: OpportunityStageName[] = ["New", "Qualified", "Proposal", "Won", "Lost"];

export const STAGE_VALUE: Record<OpportunityStageName, number> = {
  New: OpportunityStage.New,
  Qualified: OpportunityStage.Qualified,
  Proposal: OpportunityStage.Proposal,
  Won: OpportunityStage.Won,
  Lost: OpportunityStage.Lost,
};

export const isClosedStage = (stage: OpportunityStageName) => stage === "Won" || stage === "Lost";
