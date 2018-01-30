import { Component, Inject, Input, OnChanges, SimpleChanges } from
    "@angular/core";
import { Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
@Component({
    selector: "answer-list",
    templateUrl: './answer-list.component.html',
    styleUrls: ['./answer-list.component.css']
})
export class AnswerListComponent implements OnChanges {
    @Input() question : Question;
    answers: Answer[];
    title: string;

    constructor(private http: HttpClient,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string) {

        this.answers = [];
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (typeof changes['question'] !== 'undefined') {

            const change = changes['question'];

            if (!change.isFirstChange()) {
                this.loadData();
            }
        }
    }

    loadData() {
        const url = this.baseUrl + `api/answer/all/${this.question.Id}`;
        this.http.get<Answer[]>(url).subscribe(result => {
                this.answers = result;
            },
            error => console.error(error));
    }

    onCreate() {
        this.router.navigate(['/answer/create', this.question.Id]);
    }

    onEdit(answer: Answer) {
        this.router.navigate(['/answer/edit', answer.Id]);
    }

    onDelete(answer: Answer) {
        if (confirm("Do you really want to delete this answer?")) {
            const url = this.baseUrl + "api/answer/" + answer.Id;
            this.http.delete(url)
                .subscribe(result => {
                        this.loadData();
                    },
                    error => console.log(error));
        }
    }
}