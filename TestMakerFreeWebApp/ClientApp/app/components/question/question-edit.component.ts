import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'question-edit',
    templateUrl: './question-edit.component.html',
    styleUrls: ['./question-edit.component.css']
})
export class QuestionEditComponent {
    title: string;
    question: Question;
    form: FormGroup;

    editMode: boolean;

    constructor(private activatedRoute: ActivatedRoute,
        private http: HttpClient,
        private fb: FormBuilder,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string) {

        this.question = <Question>{};

        this.createForm();

        const id = +this.activatedRoute.snapshot.params['id'];

        this.editMode = this.activatedRoute.snapshot.url[1].path === 'edit';

        if (this.editMode) {
            this.title = 'Edit Question';

            const url = this.baseUrl + `api/question/${id}`;
            this.http.get<Question>(url).subscribe(result => {
                this.question = result;
                    this.updateForm();
                },
                error => console.error(error));
        } else {
            this.question.QuizId = id;
            this.title = 'Create a new Question';
        }

    }

    onSubmit(question: Question) {
        const url = this.baseUrl + 'api/question';

        const tempQuestion = <Question>{};

        tempQuestion.Text = this.form.value.Text;
        tempQuestion.QuizId = this.question.QuizId;

        if (this.editMode) {
            tempQuestion.Id = this.question.Id;
            this.http
                .put<Question>(url, tempQuestion)
                .subscribe(result => {
                    this.router.navigate(['quiz/edit', result.QuizId]);
                },
                error => console.error(error));
        } else {
            this.http
                .post<Question>(url, tempQuestion)
                .subscribe(result => {
                    this.router.navigate(['quiz/edit', result.QuizId]);
                },
                error => console.log(error));
        }
    }

    onBack() {
        this.router.navigate(['quiz/edit', this.question.QuizId]);
    }

    createForm() {
        this.form = this.fb.group({
            Text: ['', Validators.required]
        });
    }

    updateForm() {
        this.form.setValue({
            Text: this.question.Text,
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