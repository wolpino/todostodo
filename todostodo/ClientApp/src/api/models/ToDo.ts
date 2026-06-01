import type { Entry } from './Entry';

export interface ToDo extends Entry {
    dueDate?: string;
    dueTime?: string;
}
