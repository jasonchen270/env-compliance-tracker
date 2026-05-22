import { useEffect, useState } from "react";
import { api } from "./api";
import type { Facility, FacilitySummary, WasteRecord, EmissionsRecord, AuditEntry } from "./api";

type Tab = "summary" | "waste" | "emissions" | "audit";

export default function App() {
  const [user, setUser] = useState<{ username: string; role: string } | null>(() => {
    const u = localStorage.getItem("user");
    return u ? JSON.parse(u) : null;
  });

  if (!user) return <Login onLogin={(u) => { localStorage.setItem("user", JSON.stringify(u)); setUser(u); }} />;
  return <Dashboard user={user} onLogout={() => { localStorage.clear(); setUser(null); }} />;
}

function Login({ onLogin }: { onLogin: (u: { username: string; role: string }) => void }) {
  const [u, setU] = useState("officer");
  const [p, setP] = useState("officer");
  const [err, setErr] = useState("");
  return (
    <div style={{ maxWidth: 320, margin: "80px auto", fontFamily: "sans-serif" }}>
      <h2>Environmental Compliance Tracker</h2>
      <input placeholder="username" value={u} onChange={e => setU(e.target.value)} style={{ width: "100%", padding: 8, marginBottom: 8 }} />
      <input placeholder="password" type="password" value={p} onChange={e => setP(e.target.value)} style={{ width: "100%", padding: 8, marginBottom: 8 }} />
      <button onClick={async () => {
        try {
          const r = await api.login(u, p);
          localStorage.setItem("token", r.token);
          onLogin({ username: r.username, role: r.role });
        } catch (e: any) { setErr(e.message); }
      }} style={{ width: "100%", padding: 8 }}>Sign in</button>
      {err && <p style={{ color: "crimson" }}>{err}</p>}
      <p style={{ fontSize: 12, color: "#666" }}>Try admin/admin, officer/officer, viewer/viewer</p>
    </div>
  );
}

function Dashboard({ user, onLogout }: { user: { username: string; role: string }; onLogout: () => void }) {
  const [tab, setTab] = useState<Tab>("summary");
  const [facilities, setFacilities] = useState<Facility[]>([]);
  useEffect(() => { api.facilities().then(setFacilities).catch(console.error); }, []);

  const canEdit = user.role === "Admin" || user.role === "ComplianceOfficer";

  return (
    <div style={{ fontFamily: "sans-serif", maxWidth: 1100, margin: "20px auto", padding: 20 }}>
      <header style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h2>Compliance Tracker</h2>
        <div>
          <span style={{ marginRight: 12 }}>{user.username} ({user.role})</span>
          <button onClick={onLogout}>Logout</button>
        </div>
      </header>
      <nav style={{ marginBottom: 16 }}>
        {(["summary", "waste", "emissions", "audit"] as Tab[]).map(t => (
          <button key={t} onClick={() => setTab(t)} style={{ marginRight: 8, fontWeight: tab === t ? "bold" : "normal" }}>{t}</button>
        ))}
      </nav>
      {tab === "summary" && <SummaryView />}
      {tab === "waste" && <WasteView facilities={facilities} canEdit={canEdit} />}
      {tab === "emissions" && <EmissionsView facilities={facilities} canEdit={canEdit} />}
      {tab === "audit" && <AuditView />}
    </div>
  );
}

function SummaryView() {
  const [rows, setRows] = useState<FacilitySummary[]>([]);
  useEffect(() => { api.facilitySummary().then(setRows).catch(console.error); }, []);
  return (
    <table style={{ width: "100%", borderCollapse: "collapse" }}>
      <thead><tr><Th>Facility</Th><Th>Waste (kg)</Th><Th>Emissions (tCO2e)</Th><Th>Waste records</Th><Th>Emissions records</Th></tr></thead>
      <tbody>{rows.map(r => (
        <tr key={r.facilityId}><Td>{r.facilityName}</Td><Td>{r.totalWasteKg}</Td><Td>{r.totalEmissionsTonsCO2e}</Td><Td>{r.wasteRecordCount}</Td><Td>{r.emissionsRecordCount}</Td></tr>
      ))}</tbody>
    </table>
  );
}

function WasteView({ facilities, canEdit }: { facilities: Facility[]; canEdit: boolean }) {
  const [records, setRecords] = useState<WasteRecord[]>([]);
  const [form, setForm] = useState({ facilityId: facilities[0]?.id ?? 0, wasteType: "", quantityKg: 0, disposalMethod: "" });
  const reload = () => api.wasteRecords().then(setRecords).catch(console.error);
  useEffect(() => { reload(); }, []);
  useEffect(() => { if (facilities.length && !form.facilityId) setForm(f => ({ ...f, facilityId: facilities[0].id })); }, [facilities]);
  return (
    <div>
      {canEdit && (
        <div style={{ marginBottom: 16, padding: 12, background: "#f5f5f5" }}>
          <select value={form.facilityId} onChange={e => setForm({ ...form, facilityId: +e.target.value })}>
            {facilities.map(f => <option key={f.id} value={f.id}>{f.name}</option>)}
          </select>
          <input placeholder="Waste type" value={form.wasteType} onChange={e => setForm({ ...form, wasteType: e.target.value })} style={{ marginLeft: 8 }} />
          <input placeholder="kg" type="number" value={form.quantityKg} onChange={e => setForm({ ...form, quantityKg: +e.target.value })} style={{ marginLeft: 8, width: 80 }} />
          <input placeholder="Disposal method" value={form.disposalMethod} onChange={e => setForm({ ...form, disposalMethod: e.target.value })} style={{ marginLeft: 8 }} />
          <button style={{ marginLeft: 8 }} onClick={async () => {
            await api.createWaste({ ...form, reportedOn: new Date().toISOString() });
            setForm({ ...form, wasteType: "", quantityKg: 0, disposalMethod: "" });
            reload();
          }}>Add</button>
        </div>
      )}
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead><tr><Th>When</Th><Th>Facility</Th><Th>Type</Th><Th>kg</Th><Th>Disposal</Th></tr></thead>
        <tbody>{records.map(r => (
          <tr key={r.id}><Td>{r.reportedOn.slice(0, 10)}</Td><Td>{facilities.find(f => f.id === r.facilityId)?.name ?? r.facilityId}</Td><Td>{r.wasteType}</Td><Td>{r.quantityKg}</Td><Td>{r.disposalMethod}</Td></tr>
        ))}</tbody>
      </table>
    </div>
  );
}

function EmissionsView({ facilities, canEdit }: { facilities: Facility[]; canEdit: boolean }) {
  const [records, setRecords] = useState<EmissionsRecord[]>([]);
  const [form, setForm] = useState({ facilityId: facilities[0]?.id ?? 0, pollutant: "", quantityTonsCO2e: 0 });
  const reload = () => api.emissionsRecords().then(setRecords).catch(console.error);
  useEffect(() => { reload(); }, []);
  useEffect(() => { if (facilities.length && !form.facilityId) setForm(f => ({ ...f, facilityId: facilities[0].id })); }, [facilities]);
  return (
    <div>
      {canEdit && (
        <div style={{ marginBottom: 16, padding: 12, background: "#f5f5f5" }}>
          <select value={form.facilityId} onChange={e => setForm({ ...form, facilityId: +e.target.value })}>
            {facilities.map(f => <option key={f.id} value={f.id}>{f.name}</option>)}
          </select>
          <input placeholder="Pollutant" value={form.pollutant} onChange={e => setForm({ ...form, pollutant: e.target.value })} style={{ marginLeft: 8 }} />
          <input placeholder="tCO2e" type="number" value={form.quantityTonsCO2e} onChange={e => setForm({ ...form, quantityTonsCO2e: +e.target.value })} style={{ marginLeft: 8, width: 80 }} />
          <button style={{ marginLeft: 8 }} onClick={async () => {
            await api.createEmissions({ ...form, reportedOn: new Date().toISOString() });
            setForm({ ...form, pollutant: "", quantityTonsCO2e: 0 });
            reload();
          }}>Add</button>
        </div>
      )}
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead><tr><Th>When</Th><Th>Facility</Th><Th>Pollutant</Th><Th>tCO2e</Th></tr></thead>
        <tbody>{records.map(r => (
          <tr key={r.id}><Td>{r.reportedOn.slice(0, 10)}</Td><Td>{facilities.find(f => f.id === r.facilityId)?.name ?? r.facilityId}</Td><Td>{r.pollutant}</Td><Td>{r.quantityTonsCO2e}</Td></tr>
        ))}</tbody>
      </table>
    </div>
  );
}

function AuditView() {
  const [rows, setRows] = useState<AuditEntry[]>([]);
  useEffect(() => { api.audit().then(setRows).catch(console.error); }, []);
  return (
    <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 12 }}>
      <thead><tr><Th>Time</Th><Th>User</Th><Th>Entity</Th><Th>Id</Th><Th>Action</Th><Th>Before</Th><Th>After</Th></tr></thead>
      <tbody>{rows.map(r => (
        <tr key={r.id}>
          <Td>{r.timestamp}</Td><Td>{r.username}</Td><Td>{r.entityName}</Td><Td>{r.entityId}</Td><Td>{r.action}</Td>
          <Td><pre style={{ margin: 0, maxWidth: 220, overflow: "auto" }}>{r.before}</pre></Td>
          <Td><pre style={{ margin: 0, maxWidth: 220, overflow: "auto" }}>{r.after}</pre></Td>
        </tr>
      ))}</tbody>
    </table>
  );
}

const Th = ({ children }: { children: React.ReactNode }) => <th style={{ textAlign: "left", borderBottom: "1px solid #ccc", padding: 6 }}>{children}</th>;
const Td = ({ children }: { children: React.ReactNode }) => <td style={{ borderBottom: "1px solid #eee", padding: 6 }}>{children}</td>;
