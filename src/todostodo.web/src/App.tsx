import { useEffect, useState } from "react";
import { getApiEntry, postApiEntry } from "./api/generated/sdk.gen";
import { getManageInfo, postLogin, postRegister } from "./api/generated/sdk.gen";
import type { Entry } from "./api/generated/types.gen";

type AuthMode = "login" | "register";

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean | null>(null);
  const [authMode, setAuthMode] = useState<AuthMode>("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [authError, setAuthError] = useState<string | null>(null);

  const [entries, setEntries] = useState<Entry[]>([]);
  const [title, setTitle] = useState("");

  useEffect(() => {
    getManageInfo().then(({ response }) => {
      setIsAuthenticated(response.ok);
    });
  }, []);

  async function handleAuth() {
    setAuthError(null);
    if (authMode === "register") {
      const { response } = await postRegister({ body: { email, password } });
      if (!response.ok) {
        setAuthError("Registration failed. Try a stronger password (6+ chars, digit, uppercase).");
        return;
      }
    }
    const { response } = await postLogin({
      query: { useCookies: true },
      body: { email, password },
    });
    if (response.ok) {
      setIsAuthenticated(true);
      setEmail("");
      setPassword("");
    } else {
      setAuthError("Invalid email or password.");
    }
  }

  async function handleLogout() {
    await fetch("/logout", { method: "POST" });
    setIsAuthenticated(false);
    setEntries([]);
  }

  async function loadEntries() {
    const { data } = await getApiEntry();
    if (data) setEntries(data);
  }

  async function addEntry() {
    if (!title.trim()) return;
    await postApiEntry({ body: { title, description: "", status: "Active" } });
    setTitle("");
    await loadEntries();
  }

  useEffect(() => {
    if (isAuthenticated) loadEntries();
  }, [isAuthenticated]);

  if (isAuthenticated === null) return <p>Loading...</p>;

  if (!isAuthenticated) {
    return (
      <div>
        <h1>Todo App</h1>
        <h2>{authMode === "login" ? "Login" : "Register"}</h2>

        <div>
          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>
        <div>
          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </div>

        {authError && <p style={{ color: "red" }}>{authError}</p>}

        <button onClick={handleAuth}>
          {authMode === "login" ? "Login" : "Register"}
        </button>

        <p>
          {authMode === "login" ? "No account? " : "Already have an account? "}
          <button onClick={() => { setAuthMode(authMode === "login" ? "register" : "login"); setAuthError(null); }}>
            {authMode === "login" ? "Register" : "Login"}
          </button>
        </p>
      </div>
    );
  }

  return (
    <div>
      <h1>Todo App</h1>
      <button onClick={handleLogout}>Logout</button>

      <div>
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="New entry..."
        />
        <button onClick={addEntry}>Add</button>
      </div>

      <ul>
        {entries.map((entry) => (
          <li key={entry.id}>{entry.title}</li>
        ))}
      </ul>
    </div>
  );
}

export default App;
