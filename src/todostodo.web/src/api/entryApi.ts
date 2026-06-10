export type EntryStatus = "active" | "inprogress" | "completed" | "archived" | "inactive";

export interface Entry{
  id: number;
  title: string;
  description?: string;
  status: EntryStatus;
}

const API_URL = "http://localhost:5162/api/entry";

export async function getEntries(): Promise<Entry[]> {
  const response = await fetch(API_URL);

  return response.json();
}

export async function createEntry(
  title: string,
  description: string,
  status: EntryStatus,
): Promise<void> {
  await fetch(API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      title,
      description,
      status,
    }),
  });
}