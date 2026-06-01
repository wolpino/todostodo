// These are basically signals saying "this is generated/infrastructure code, don't lint it."
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ToDo } from '../models/ToDo';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ToDoService {

    /**
     * Get all todos for the current user
     * @returns Todo Success
     * @throws ApiError
     */
    public static getApiTodos(): CancelablePromise<Array<ToDo>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/todos',
        });
    }

    /**
     * Get a specific todo by ID
     * @param id 
     * @returns Todo Success
     * @throws ApiError
     */
    public static getApiTodosById(
        id: number,
    ): CancelablePromise<ToDo> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/todos/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * Create a new todo
     * @param requestBody 
     * @returns ToDo Success
     * @throws ApiError
     */
    public static postApiToDos(
        requestBody?: {
            description: string;
            dueDate?: string;
            dueTime?: string;
        },
    ): CancelablePromise<ToDo> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/todos',
            body: requestBody,
            mediaType: 'application/json-patch+json',
        });
    }

    /**
     * Update an existing todo
     * @param id 
     * @param requestBody 
     * @returns ToDo Success
     * @throws ApiError
     */
    public static putApiToDosById(
        id: number,
        requestBody?: {
            description?: string;
            dueDate?: string;
            dueTime?: string;
            status?: 'Active' | 'Inactive';
        },
    ): CancelablePromise<ToDo> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/todos/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json-patch+json',
        });
    }

    /**
     * Delete a todo
     * @param id 
     * @returns void 
     * @throws ApiError
     */
    public static deleteApiToDosById(
        id: number,
    ): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/todos/{id}',
            path: {
                'id': id,
            },
        });
    }
}
