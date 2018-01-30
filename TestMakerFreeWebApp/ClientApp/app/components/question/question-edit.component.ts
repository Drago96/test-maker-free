import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'question-edit',
    templateUrl: './question-edit.component.html',
    styleUrls: ['./question-edit.component.css']
})
export class QuestionEditComponent {
    title: string;
    question: Question;

    editMode: boolean;

    constructor(private activatedRoute: ActivatedRoute,
        private http: HttpClient,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string) {

        this.question = <Question>{};

        const id = +this.activatedRoute.snapshot.params['id'];

        this.editMode = this.activatedRoute.snapshot.url[1].path === 'edit';

        if (this.editMode) {
            this.title = 'Edit Question'

            const url = this.baseUrl + `api/question/${id}`;
            this.http.get<Question>(url).subscribe(result => {
                    this.question = result;
                },
                error => console.error(error));
        } else {
            this.question.QuizId = id;
            this.title = 'Create a new Question'
        }

    }

    onSubmit(question: Question) {
        const url = this.baseUrl + 'api/question';

        if (this.editMode) {
            this.http
                .put<Question>(url, this.question)
                .subscribe(result => {
                        this.router.navigate(['quiz/edit', result.QuizId]);
                    },
                    error => console.error(error));
        } else {
            this.http
                .post<Question>(url, this.question)
                .subscribe(result => {
                        this.router.navigate(['quiz/edit', result.QuizId]);
                    },
                    error => console.log(error));
        }
    }

    onBack() {
        this.router.navigate(['quiz/edit', this.question.QuizId]);
    }
}