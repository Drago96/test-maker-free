import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'quiz-edit',
    templateUrl: './quiz-edit.component.html',
    styleUrls: ['./quiz-edit.component.css']
})
export class QuizEditComponent {
    title: string;
    quiz: Quiz;

    // this will be TRUE when editing an existing quiz,
    // FALSE when creating a new one.
    editMode: boolean;

    constructor(private http: HttpClient,
        private router: Router,
        private activatedRoute: ActivatedRoute,
        @Inject('BASE_URL') private baseUrl: string) {

        this.quiz = <Quiz>{};

        const id = +this.activatedRoute.snapshot.params['id'];
        if (id) {
            this.editMode = true;

            // fetch the quiz from the server
            const url = this.baseUrl + `/api/quiz/${id}`;
            this.http.get<Quiz>(url).subscribe(result => {
                this.quiz = result;
                this.title = `Edit - ${this.quiz.Title}`;
            });
        } else {
            this.editMode = false;
            this.title = "Create a new Quiz";
        }
    }

    onSubmit(quiz: Quiz) {
        const url = this.baseUrl + '/api/quiz';

        if (this.editMode) {
            this.http
                .put(url, quiz)
                .subscribe(res => {
                        this.router.navigate(['home']);
                    },
                    error => console.error(error));
        } else {
            this.http
                .post(url, quiz)
                .subscribe(res => {
                    this.router.navigate(['home']);
                }, error => console.log(error));
        }
    }

    onBack() {
        this.router.navigate(['home']);
    }
}
