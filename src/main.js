const { invoke } = window.__TAURI__.tauri;
const { open } = window.__TAURI__.dialog;

function scrollToPage(pageid) {
    window.scrollTo({top: document.getElementById(pageid).offsetTop, behavior: 'smooth'});
}

async function download_file() {
    try {
        const response = await invoke('download_file');
        if (response === 'exists') {
            alert('File already exists');
            scrollToPage('page5');
        }
        if (response.toString() === 'notfound') {
            scrollToPage('page4');
        }
        else {
            scrollToPage('page3');
        }
        console.log(response);
    } catch (error) {
        console.error(error);
    }
}

async function install() {
    const inputPath = document.getElementById("input_path").value;

    if (!inputPath) {
        alert("Please enter a path");
        return;
    }

    console.log(inputPath);

    const response = await invoke('download_file', {kspGivenPath: inputPath});


    if (response === 'exists') {
        alert('File already exists');
        scrollToPage('page5');
        return;
    }
    if (response === 'not-valid') {
        alert('Path is not a valid KSP2 install');
        return;
    }
    if (response === 'notfound') {
        scrollToPage('page4');
        return;
    }
    else {
        scrollToPage('page3');
    }
    console.log(response);
}

async function select_folder() {
    try {
        const result = await open({
            directory: true,
            multiple: false
        });
        if (result === null || result.length === 0) {
            console.log('No folder selected');
            return;
        }
        document.getElementById('input_path').value = result;
    } catch (error) {
        console.error(error);
    }
}
