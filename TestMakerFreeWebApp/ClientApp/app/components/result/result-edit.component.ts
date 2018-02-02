import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'result-edit',
    templateUrl: './result-edit.component.html',
    styleUrls: ['./result-edit.component.css']
})
export class ResultEditComponent {
    title: string;
    result: Result;

    editMode: boolean;

    constructor(private activatedRoute: ActivatedRoute,
        private http: HttpClient,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string) {

        this.result = <Result>{};

        const id = +this.activatedRoute.snapshot.params['id'];

        this.editMode = this.activatedRoute.snapshot.url[1].path === 'edit';

        if (this.editMode) {
            this.title = 'Edit Result';

            const url = this.baseUrl + `api/result/${id}`;
            this.http.get<Result>(url).subscribe(result => {
                this.result = result;
            },
                error => console.error(error));
        } else {
            this.result.QuizId = id;
            this.title = 'Create a new Result';
        }

    }

    onSubmit(result: Result) {
        const url = this.baseUrl + 'api/result';

        if (this.editMode) {
            this.http
                .put<Result>(url, this.result)
                .subscribe(result => {
                    this.router.navigate(['quiz/edit', result.QuizId]);
                },
                error => console.error(error));
        } else {
            this.http
                .post<Result>(url, this.result)
                .subscribe(result => {
                    this.router.navigate(['quiz/edit', result.QuizId]);
                },
                error => console.log(error));
        }
    }

    onBack() {
        this.router.navigate(['quiz/edit', this.result.QuizId]);
    }
}