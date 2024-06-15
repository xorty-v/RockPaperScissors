const player1Status = document.querySelector('#player1-status'),
    player1Options = document.querySelectorAll('#options > svg'),
    player2Status = document.querySelector('#player2-status'),
    player2Image = document.querySelector('.player2-moves > img'),
    roundResult = document.querySelector('#result');

const connection = new signalR.HubConnectionBuilder()
    .withUrl("hub/game")
    .withAutomaticReconnect()
    .build();

let gameSessionId;
let player1Score = 0;
let player2Score = 0;
let roundNumber = 1;

player1Options.forEach(hand => {
    hand.onclick = makeMove;
});

function makeMove() {
    const hand = this;

    hand.classList.add('active--you');
    player1Status.textContent = 'You made a pick';

    player1Options.forEach(hand => {
        hand.onclick = null;
        hand.classList.remove('pick');
    });

    connection.invoke("PlayGame", gameSessionId, parseInt(hand.dataset.mean));
}

function restartRound() {
    document.querySelector('.round-counter').textContent = `ROUND ${++roundNumber}`;

    player2Status.textContent = 'Waiting for the opponent';
    player2Image.src = `./img/spinner.gif`;
    roundResult.textContent = ``;

    player1Status.textContent = 'Take your pick';
    player1Options.forEach(hand => {
        hand.classList.remove('active--you');
        hand.classList.add("pick");
        hand.onclick = makeMove;
    });
}

(async () => {
    if (window.location.href.includes('gameId')) {

        document.querySelector('.waiting__info').classList.remove('hide');

        // получаем id сгенерированный с предыдущей страницы 
        const gameId = new URL(window.location.href).searchParams.get('gameId');;

        document.querySelector('.info__link').textContent = `http://localhost/game.html?gameId=${gameId}`;

        // штука для того чтобы юзер мог кликая копировать ссылку
        document.querySelector('#copy-zone').addEventListener('click', () => {
            const text = document.querySelector('.info__link');
            navigator.clipboard.writeText(text.textContent);
            document.querySelector('.prompt').textContent = 'Copied';
        })

        await connection.start();
        await connection.invoke("JoinFriendGame", gameId);
    }
    else {
        await connection.start();
        await connection.invoke("JoinRandomGame");
    }
})();

connection.on("GameStarted", (gameId) => {
    const waiting = document.querySelector('.waiting');
    const game = document.querySelector('.main');

    waiting.classList.add('hide');
    game.classList.remove('hide');

    gameSessionId = gameId;
});

connection.on("OpponentMadeMove", () => {
    player2Status.innerHTML = 'Opponent made a pick';
    player2Image.src = './img/done.png';
});

connection.on("RoundFinished", async (result, move) => {
    player2Image.src = `./img/${move.toLowerCase()}.svg`;

    if (["win", "lose"].includes(result.toLowerCase())) {
        roundResult.textContent = `YOU ${result}`;
    }
    else {
        roundResult.textContent = `${result}`;
    }

    if (result.toLowerCase() === "win") {
        document.querySelector('.player1-count').textContent = `${++player1Score}/3`;
    }

    if (result.toLowerCase() === "lose") {
        document.querySelector('.player2-count').textContent = `${++player2Score}/3`;
    }

    setTimeout(restartRound, 2000);
});

connection.on("GameFinished", (result, move) => {
    player2Image.src = `./img/${move.toLowerCase()}.svg`;

    if (["win", "lose"].includes(result.toLowerCase())) {
        roundResult.textContent = `YOU ${result}`;
    }
    else {
        roundResult.textContent = `${result}`;
    }

    if (result.toLowerCase() === "win") {
        document.querySelector('.player1-count').textContent = `${++player1Score}/3`;
    }

    if (result.toLowerCase() === "lose") {
        document.querySelector('.player2-count').textContent = `${++player2Score}/3`;
    }

    setTimeout(() => {
        document.querySelector('.modal').classList.remove('hide');

        document.querySelector('.modal-title').textContent = `You ${result}`;
        document.querySelector('.modal-score').textContent = `${player1Score}/3`;
    }, 2000);
});