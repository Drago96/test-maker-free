import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'answer-edit',
    templateUrl: './answer-edit.component.html',
    styleUrls: ['./answer-edit.component.css']
})
export class AnswerEditComponent {
    title: string;
    answer: Answer;

    editMode: boolean;

    constructor(private activatedRoute: ActivatedRoute,
        private http: HttpClient,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string) {

        this.answer = <Answer>{};

        const id = +this.activatedRoute.snapshot.params['id'];

        this.editMode = this.activatedRoute.snapshot.url[1].path === 'edit';

        if (this.editMode) {
            this.title = 'Edit Answer';

            const url = this.baseUrl + `api/answer/${id}`;
            this.http.get<Answer>(url).subscribe(result => {
                this.answer = result;
            },
                error => console.error(error));
        } else {
            this.answer.QuestionId = id;
            this.title = 'Create a new Answer';
        }

    }

    onSubmit(answer: Answer) {
        const url = this.baseUrl + 'api/answer';

        if (this.editMode) {
            this.http
                .put<Answer>(url, this.answer)
                .subscribe(result => {
                    this.router.navigate(['question/edit', result.QuestionId]);
                },
                error => console.error(error));
        } else {
            this.http
                .post<Answer>(url, this.answer)
                .subscribe(result => {
                    this.router.navigate(['question/edit', result.QuestionId]);
                },
                error => console.log(error));
        }
    }

    onBack() {
        this.router.navigate(['question/edit', this.answer.QuestionId]);
    }
}