import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import type { Employee } from '../employees/employee.models';

@Service()
export class HomeService {
    private http = inject(HttpClient);

    getAll() {
        return this.http.get<Employee[]>('/api/employees');
    }
}
