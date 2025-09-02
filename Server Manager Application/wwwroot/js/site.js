const delay = (ms) => new Promise(resolve => setTimeout(resolve, ms));


async function startLiveClock(element) {
    while (true) {
        element.textContent = new Date().toLocaleString();

        await new Promise(resolve => setTimeout(resolve, 1000));
    };
}

async function fadeIn(element, duration = 500) {
    return new Promise(resolve => {
        let opacity = 0;

        element.style.opacity = 0;
        element.style.display = 'block';

        let startTime = null;

        function animate(currentTime) {
            if (!startTime) startTime = currentTime;

            const progress = (currentTime - startTime) / duration;
            opacity = Math.min(progress, 1);
            element.style.opacity = opacity;

            if (progress < 1) {
                requestAnimationFrame(animate);

            } else {
                resolve();
            }
        }

        requestAnimationFrame(animate);
    });
}

async function fadeOut(element, duration = 500) {
    return new Promise(resolve => {
        let opacity = 1;
        element.style.opacity = 1;

        let startTime = null;

        function animate(currentTime) {
            if (!startTime) startTime = currentTime;

            const progress = (currentTime - startTime) / duration;
            opacity = Math.max(1 - progress, 0);
            element.style.opacity = opacity;

            if (progress < 1) {
                requestAnimationFrame(animate);

            } else {
                element.style.display = 'none';
                resolve();
            }
        }

        requestAnimationFrame(animate);
    });
}

async function handleAboutPopUp() {
    spawnAboutElement();

    const element = document.getElementById("widget_10");
    const duration = 500;

    await fadeIn(element, duration);
    await delay(5000);
    await fadeOut(element, duration);

    element.remove();

    document.getElementById("app_about_button").addEventListener('click', handleAboutPopUp);
}

function handleCloseMessage(element) {
    element.remove();
}

async function main()
{
    const time_clock_element = document.getElementById("a_time");
    const about_btn = document.getElementById("app_about_button");

    if (about_btn !== null)
    {
        about_btn.addEventListener('click', handleAboutPopUp);
    }

    if (time_clock_element !== null)
    {
        await startLiveClock(time_clock_element);
    }
}

main();