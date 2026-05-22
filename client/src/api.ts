const BASE = "http://localhost:5080";

export interface LoginResponse { token: string; username: string; role: string; }
export interface Facility { id: number; name: string; location: string; }
export interface WasteRecord { id: number; facilityId: number; reportedOn: string; wasteType: string; quantityKg: number; disposalMethod: string; }
export interface EmissionsRecord { id: number; facilityId: number; reportedOn: string; pollutant: string; quantityTonsCO2e: number; }
export interface AuditEntry { id: number; timestamp: string; username: string; entityName: string; entityId: string; action: string; before?: string; after?: string; }
export interface FacilitySummary { facilityId: number; facilityName: string; totalWasteKg: number; totalEmissionsTonsCO2e: number; wasteRecordCount: number; emissionsRecordCount: number; }

function token(): string | null { return localStorage.getItem("token"); }

async function req<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = { "Content-Type": "application/json", ...(init.headers as Record<string, string> ?? {}) };
  const t = token();
  if (t) headers["Authorization"] = `Bearer ${t}`;
  const resp = await fetch(`${BASE}${path}`, { ...init, headers });
  if (!resp.ok) throw new Error(`${resp.status} ${await resp.text()}`);
  if (resp.status === 204) return undefined as T;
  return resp.json();
}

export const api = {
  login: (username: string, password: string) =>
    req<LoginResponse>("/api/auth/login", { method: "POST", body: JSON.stringify({ username, password }) }),
  facilities: () => req<Facility[]>("/api/facilities"),
  createFacility: (f: Omit<Facility, "id">) => req<Facility>("/api/facilities", { method: "POST", body: JSON.stringify(f) }),
  wasteRecords: (facilityId?: number) => req<WasteRecord[]>(`/api/waste-records${facilityId ? `?facilityId=${facilityId}` : ""}`),
  createWaste: (r: Omit<WasteRecord, "id">) => req<WasteRecord>("/api/waste-records", { method: "POST", body: JSON.stringify(r) }),
  emissionsRecords: (facilityId?: number) => req<EmissionsRecord[]>(`/api/emissions-records${facilityId ? `?facilityId=${facilityId}` : ""}`),
  createEmissions: (r: Omit<EmissionsRecord, "id">) => req<EmissionsRecord>("/api/emissions-records", { method: "POST", body: JSON.stringify(r) }),
  facilitySummary: () => req<FacilitySummary[]>("/api/reports/facility-summary"),
  audit: (entityName?: string, entityId?: string) => {
    const qs = new URLSearchParams();
    if (entityName) qs.set("entityName", entityName);
    if (entityId) qs.set("entityId", entityId);
    return req<AuditEntry[]>(`/api/audit?${qs}`);
  },
};
