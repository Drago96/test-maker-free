import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'answer-edit',
    templateUrl: './answer-edit.component.html',
    styleUrls: ['./answer-edit.component.css']
})
export class AnswerEditComponent {
    title: string;
    answer: Answer;
    form: FormGroup;

    editMode: boolean;

    constructor(private activatedRoute: ActivatedRoute,
        private http: HttpClient,
        private fb: FormBuilder,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string) {
        this.answer = <Answer>{};
        this.createForm();

        const id = +this.activatedRoute.snapshot.params['id'];

        this.editMode = this.activatedRoute.snapshot.url[1].path === 'edit';

        if (this.editMode) {
            this.title = 'Edit Answer';

            const url = this.baseUrl + `api/answer/${id}`;
            this.http.get<Answer>(url).subscribe(result => {
                this.answer = result;
                this.updateForm();
            },
                error => console.error(error));
        } else {
            this.answer.QuestionId = id;
            this.title = 'Create a new Answer';
        }
    }

    onSubmit(answer: Answer) {
        const url = this.baseUrl + 'api/answer';

        const tempAnswer = <Answer>{};
        tempAnswer.Text = this.form.value.Text;
        tempAnswer.Value = this.form.value.Value;
        tempAnswer.QuestionId = this.answer.QuestionId;

        if (this.editMode) {
            tempAnswer.Id = this.answer.Id;
            this.http
                .put<Answer>(url, tempAnswer)
                .subscribe(result => {
                    this.router.navigate(['question/edit', result.QuestionId]);
                },
                error => console.error(error));
        } else {
            this.http
                .post<Answer>(url, tempAnswer)
                .subscribe(result => {
                    this.router.navigate(['question/edit', result.QuestionId]);
                },
                error => console.log(error));
        }
    }

    onBack() {
        this.router.navigate(['question/edit', this.answer.QuestionId]);
    }

    createForm() {
        this.form = this.fb.group({
            Text: ['', Validators.required],
            Value: ['', [Validators.required, Validators.min(-5), Validators.max(5)]]
        });
    }

    updateForm() {
        this.form.setValue({
            Text: this.answer.Text,
            Value: this.answer.Value
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