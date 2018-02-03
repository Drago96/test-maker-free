import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'result-edit',
    templateUrl: './result-edit.component.html',
    styleUrls: ['./result-edit.component.css']
})
export class ResultEditComponent {
    title: string;
    result: Result;
    form: FormGroup;

    editMode: boolean;

    constructor(private activatedRoute: ActivatedRoute,
        private http: HttpClient,
        private router: Router,
        private fb: FormBuilder,
        @Inject('BASE_URL') private baseUrl: string) {

        this.result = <Result>{};
        this.createForm();

        const id = +this.activatedRoute.snapshot.params['id'];

        this.editMode = this.activatedRoute.snapshot.url[1].path === 'edit';

        if (this.editMode) {
            this.title = 'Edit Result';

            const url = this.baseUrl + `api/result/${id}`;
            this.http.get<Result>(url).subscribe(result => {
                this.result = result;
                    this.updateForm();
                },
                error => console.error(error));
        } else {
            this.result.QuizId = id;
            this.title = 'Create a new Result';
        }

    }

    onSubmit() {
        const url = this.baseUrl + 'api/result';

        const tempResult = <Result>{};
        tempResult.Text = this.form.value.Text;
        tempResult.MinValue = this.form.value.MinValue;
        tempResult.MaxValue = this.form.value.MaxValue;
        tempResult.QuizId = this.result.QuizId;

        if (this.editMode) {
            tempResult.Id = this.result.Id;
            this.http
                .put<Result>(url, tempResult)
                .subscribe(result => {
                    this.router.navigate(['quiz/edit', result.QuizId]);
                },
                error => console.error(error));
        } else {
            this.http
                .post<Result>(url, tempResult)
                .subscribe(result => {
                    this.router.navigate(['quiz/edit', result.QuizId]);
                },
                error => console.log(error));
        }
    }

    onBack() {
        this.router.navigate(['quiz/edit', this.result.QuizId]);
    }

    createForm() {
        this.form = this.fb.group({
            Text: ['', Validators.required],
            MinValue: ['', Validators.pattern(/^\d*$/)],
            MaxValue: ['', Validators.pattern(/^\d*$/)]
        });
    }

    updateForm() {
        this.form.setValue({
            Text: this.result.Text,
            MinValue: this.result.MinValue || '',
            MaxValue: this.result.MaxValue || ''
        });
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