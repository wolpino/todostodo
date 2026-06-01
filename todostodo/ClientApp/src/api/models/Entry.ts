// why use an interface here:
// because TypeScript interfaces are compile-time contracts for API response shapes
// for API contracts: interface is simpler and cleaner. Classes add complexity without benefit for plain data structures.
// --API returns plain JSON (objects), not class instances
// Interface just adds type safety at compile-time
// No runtime overhead or construction needed
//More idiomatic for HTTP APIs
export interface Entry {
    id?: number;
    description: string;
    createdDate?: Date;
    modifiedDate?: Date;
    status: 'Active' | 'Inactive';
    applicationUserId: number;
}
