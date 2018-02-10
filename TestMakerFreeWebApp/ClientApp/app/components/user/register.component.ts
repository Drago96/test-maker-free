import { Component, Inject } from '@angular/core';
import { FormGroup, FormControl, FormBuilder, Validators } from
    '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
@Component({
    selector: 'register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})
export class RegisterComponent {
    title: string;
    form: FormGroup;

    constructor(private fb: FormBuilder,
        private router: Router,
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string) {
        this.title = 'New User Registration';

        this.createForm();
    }

    createForm() {
        this.form = this.fb.group({
            Username: ['', Validators.required],
            Email: ['', [Validators.required, Validators.email]],
            Password: ['', Validators.required],
            PasswordConfirm: ['', Validators.required],
            DisplayName: ['', Validators.required]
        },
            {
                validator: this.passwordConfirmValidator
            });
    }

    onSubmit() {
        const tempUser = <User>{};

        tempUser.Username = this.form.value.Username;
        tempUser.Email = this.form.value.Email;
        tempUser.Password = this.form.value.Password;
        tempUser.DisplayName = this.form.value.DisplayName;

        const url = this.baseUrl + 'api/user';

        this.http
            .post<User>(url, tempUser)
            .subscribe(res => {
                if (res) {
                    this.router.navigate(['login']);
                } else {
                    this.form.setErrors({
                        'register': 'User registration failed.'
                    });
                }
            },
            error => console.error(error));
    }

    onBack() {
        this.router.navigate(['home']);
    }

    passwordConfirmValidator(control: FormControl): any {
        const p = control.root.get('Password');
        const pc = control.root.get('PasswordConfirm');

        if (p && pc) {
            if (p.value !== pc.value) {
                pc.setErrors({
                    'PasswordMismatch': true
                });
            } else {
                pc.setErrors(null);
            }
        }

        return null;
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