import { fail } from 'assert';
import type { Actions, PageServerLoad } from './$types';
import { API_BASE_URL } from '$lib/config';

export interface TodoItem {
    id: number;
    title: string;
    description: string;
    isComplete: boolean;
    createdAt: Date;
}

export const load: PageServerLoad = async ({ fetch }) => {

    const response = await fetch(`${API_BASE_URL}/todos`);
    if (!response.ok) {
        throw new Error('Failed to fetch todos from API service.');
    }

    const todos: TodoItem[] = await response.json();

    return {
        serverMessage: 'This is server-side data fetched from +page.server.ts',
        todos
    };
};

export const actions: Actions = {
    addTodo: async ({ request }) => {
        const data = await request.formData();
        const title = data.get("title") as string;
        const description = data.get("description") as string;

        if (!title || !description) {
            return fail('Title and Description are required.');
        }

        try {

            const response = await fetch(`${API_BASE_URL}/todos`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ title: title.trim(), description: description.trim() })
            });

            if (!response.ok) {
                console.error('Failed to add todo item:', await response.text());
                return fail('Failed to add todo item.');
            }

            return { success: true, error: false };

        } catch (error) {
            console.error('Error adding todo item:', error);
            return fail('Failed to add todo item.');
        }
    },

    deleteTodo: async ({ request }) => {
        const data = await request.formData();
        const id = data.get("id") as string;

        if (!id) {
            return fail('ID is required.');
        }

        try {
            const response = await fetch(`${API_BASE_URL}/todos/${id}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                return fail('Failed to delete todo item.');
            }

            return { success: true };

        } catch (error) {
            return fail('Failed to delete todo item.');
        }
    },
    
    refresh: async () => {
        return { success: true };
    }
};
