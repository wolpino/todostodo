// These are basically signals saying "this is generated/infrastructure code, don't lint it."
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Task } from '../models/Task';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TasksService {

    /**
     * Get all tasks for the current user
     * @returns Task Success
     * @throws ApiError
     */
    public static getApiTasks(): CancelablePromise<Array<Task>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tasks',
        });
    }

    /**
     * Get a specific task by ID
     * @param id 
     * @returns Task Success
     * @throws ApiError
     */
    public static getApiTasksById(
        id: number,
    ): CancelablePromise<Task> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tasks/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * Create a new task
     * @param requestBody 
     * @returns Task Success
     * @throws ApiError
     */
    public static postApiTasks(
        requestBody?: {
            description: string;
            dueDate?: string;
            dueTime?: string;
        },
    ): CancelablePromise<Task> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/tasks',
            body: requestBody,
            mediaType: 'application/json-patch+json',
        });
    }

    /**
     * Update an existing task
     * @param id 
     * @param requestBody 
     * @returns Task Success
     * @throws ApiError
     */
    public static putApiTasksById(
        id: number,
        requestBody?: {
            description?: string;
            dueDate?: string;
            dueTime?: string;
            status?: 'Active' | 'Inactive';
        },
    ): CancelablePromise<Task> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/tasks/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json-patch+json',
        });
    }

    /**
     * Delete a task
     * @param id 
     * @returns void 
     * @throws ApiError
     */
    public static deleteApiTasksById(
        id: number,
    ): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/tasks/{id}',
            path: {
                'id': id,
            },
        });
    }
}
