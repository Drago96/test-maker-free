import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'quiz-edit',
    templateUrl: './quiz-edit.component.html',
    styleUrls: ['./quiz-edit.component.css']
})
export class QuizEditComponent {
    title: string;
    quiz: Quiz;
    form: FormGroup;

    // this will be TRUE when editing an existing quiz,
    // FALSE when creating a new one.
    editMode: boolean;

    constructor(private http: HttpClient,
        private router: Router,
        private activatedRoute: ActivatedRoute,
        private fb: FormBuilder,
        @Inject('BASE_URL') private baseUrl: string) {

        this.quiz = <Quiz>{};

        this.createForm();

        const id = +this.activatedRoute.snapshot.params['id'];
        if (id) {
            this.editMode = true;

            // fetch the quiz from the server
            const url = this.baseUrl + `/api/quiz/${id}`;
            this.http.get<Quiz>(url).subscribe(result => {
                this.quiz = result;
                this.title = `Edit - ${this.quiz.Title}`;

                this.updateForm();
            });
        } else {
            this.editMode = false;
            this.title = "Create a new Quiz";
        }
    }

    createForm() {
        this.form = this.fb.group({
            Title: ['', Validators.required],
            Description: '',
            Text: ''
        });
    }

    updateForm() {
        this.form.setValue({
            Title: this.quiz.Title,
            Description: this.quiz.Description || '',
            Text: this.quiz.Text || ''
        });
    }

    onSubmit() {
        const url = this.baseUrl + '/api/quiz';

        const tempQuiz = <Quiz>{};
        tempQuiz.Title = this.form.value.Title;
        tempQuiz.Description = this.form.value.Description;
        tempQuiz.Text = this.form.value.Text;

        if (this.editMode) {
            tempQuiz.Id = this.quiz.Id;
            this.http
                .put(url, tempQuiz)
                .subscribe(res => {
                    this.router.navigate(['home']);
                },
                error => console.error(error));
        } else {
            this.http
                .post(url, tempQuiz)
                .subscribe(res => {
                    this.router.navigate(['home']);
                }, error => console.log(error));
        }
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
