import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'quiz',
    templateUrl:'./quiz.component.html',
    styleUrls:['./quiz.component.css']
})
export class QuizComponent {
    quiz: Quiz;

    constructor(private http: HttpClient,
        private activatedRoute: ActivatedRoute,
        private router: Router,
        @Inject('BASE_URL') private baseUrl:string) {

        this.quiz = <Quiz>{};

        const id = +this.activatedRoute.snapshot.params["id"];

        if (id) {
            const url = `${baseUrl}api/quiz/${id}`;

            this.http.get<Quiz>(url).subscribe(result => {
                    this.quiz = result;
                },
                error => console.error(error));

        } else {
            this.router.navigate(['home']);
        }
    }

    onEdit() {
        this.router.navigate(["quiz/edit", this.quiz.Id]);
    }

    onDelete() {
        if (confirm('Do you really want to delete this quiz?')) {
            const url = this.baseUrl + `api/quiz/${this.quiz.Id}`;
            this.http.delete(url)
                .subscribe(result => {
                        this.router.navigate(['home']);
                    },
                    error => console.error(error));
        }
    }
}