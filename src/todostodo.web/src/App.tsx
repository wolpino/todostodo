import { useEffect, useState } from "react";
import { getApiEntry, postApiEntry } from "./api/generated/sdk.gen";
import type { Entry } from "./api/generated/types.gen";

function App() {
  const [entries, setEntries] = useState<Entry[]>([]);
  const [title, setTitle] = useState("");

  async function loadEntries() {
    const { data } = await getApiEntry();
    if (data) setEntries(data);
  }

  async function addEntry() {
    if (!title.trim()) return;

    await postApiEntry({
      body: { title, description: "", status: "Active" },
    });

    setTitle("");
    await loadEntries();
  }

  useEffect(() => {
    loadEntries();
  }, []);

  return (
    <>
      <h1>Todo App</h1>

      <input
        value={title}
        onChange={(e) => setTitle(e.target.value)}
      />

      <button onClick={addEntry}>Add</button>

      <ul>
        {entries.map((entry) => (
          <li key={entry.id}>{entry.title}</li>
        ))}
      </ul>
    </>
  );
}

export default App;
