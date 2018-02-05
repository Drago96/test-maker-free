import { Component, Inject } from '@angular/core';
import { FormGroup, FormControl, FormBuilder, Validators } from
    '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent {
    title: string;
    form: FormGroup;

    constructor(private router: Router,
        private fb: FormBuilder,
        private authService: AuthService,
        @Inject('BASE_URL') private baseUrl: string) {
        this.title = 'User Login';
        this.createForm();
    }

    createForm() {
        this.form = this.fb.group({
            Username: ['', Validators.required],
            Password: ['', Validators.required]
        });
    }

    onSubmit() {
        const username = this.form.value.Username;
        const password = this.form.value.Password;

        this.authService.login(username, password)
            .subscribe(res => {
                this.router.navigate(['home']);
            },
            error => this.form.setErrors({
                'auth': 'Incorrect username or password'
            }));
    }

    onBack() {
        this.router.navigate(['home']);
    }

    hasError(name: string) {
        const hasError = this.isChanged(name) && !this.isValid(name);
        return hasError;
    }

    private getFormControl(name: string) {
        return this.form.get(name);
    }

    private isValid(name: string) {
        const control = this.getFormControl(name);
        return control && control.valid;
    }

    private isChanged(name: string) {
        const control = this.getFormControl(name);
        return control && (control.dirty || control.touched);
    }
}