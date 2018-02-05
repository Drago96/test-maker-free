import { Component, Inject, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router'
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'quiz-list',
    templateUrl: './quiz-list.component.html',
    styleUrls: ['./quiz-list.component.css']
})
export class QuizListComponent implements OnInit {
    @Input() class: string;
    title: string;
    selectedQuiz: Quiz;
    quizzes: Quiz[];

    constructor(private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string,
        private router: Router) {
    }

    ngOnInit(): void {
        this.title = 'Latest Quizzes';
        let url = this.baseUrl + 'api/quiz/';

        switch (this.class) {
            case 'byTitle':
                this.title = 'Quizzes by Title';
                url += 'byTitle/';
                break;
            case 'random':
                this.title = 'Random Quizzes';
                url += 'random/';
                break;

            case 'latest':
            default:
                this.title = 'Latest Quizzes';
                url += 'latest/';
                break;
        }

        this.http.get<Quiz[]>(url).subscribe(result => {
            this.quizzes = result;
        },
            error => console.error(error));
    }

    onSelect(quiz: Quiz) {
        this.selectedQuiz = quiz;
        this.router.navigate(['quiz', this.selectedQuiz.Id]);
    }
}