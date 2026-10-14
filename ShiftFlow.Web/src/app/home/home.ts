import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { Auth } from '../auth/auth';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
})
export class Home {
  private router = inject(Router);
  protected readonly auth = inject(Auth);

  protected logout() {
    this.auth.logout().subscribe({
      complete: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login']),
    });
  }
}
