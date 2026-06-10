import { useEffect, useState } from "react";
import {
  createEntry, type Entry as Entry, getEntries,
} from "./api/entryApi";

function App() {
  const [entries, setEntries] =
    useState<Entry[]>([]);

  const [title, setTitle] =
    useState("");

  async function loadEntries() {
    const data = await getEntries();
    setEntries(data);
  }

  async function addEntry() {
    if (!title.trim()) return;

    const description = "";

    await createEntry(title, description, "active");

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
        onChange={(e) =>
          setTitle(e.target.value)
        }
      />

      <button onClick={addEntry}>
        Add
      </button>

      <ul>
        {entries.map((entry) => (
          <li key={entry.id}>
            {entry.title}
          </li>
        ))}
      </ul>
    </>
  );
}

export default App;