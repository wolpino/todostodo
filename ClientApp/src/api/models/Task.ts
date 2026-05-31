import type { Entry } from './Entry';

export interface Task extends Entry {
    dueDate?: string;
    dueTime?: string;
}
