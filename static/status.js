function updateStatus() {
    fetch('/status-data')
        .then(r => r.text())
        .then(data => {
            console.log('Raw data:', data);
            
            const lines = data.split('\n');
            let active, accepts, handled, requests, reading, writing, waiting;

            lines.forEach(line => {
                line = line.trim();
                
                if (line.startsWith('Active connections:')) {
                    active = line.match(/\d+/)[0];
                }
                else if (line.includes('server accepts handled requests')) {
                    const nextLine = lines[lines.indexOf(line) + 1];
                    const nums = nextLine.trim().match(/\d+/g);
                    if (nums && nums.length >= 3) {
                        accepts = nums[0];
                        handled = nums[1];
                        requests = nums[2];
                    }
                }
                else if (line.startsWith('Reading:')) {
                    const nums = line.match(/\d+/g);
                    if (nums && nums.length >= 3) {
                        reading = nums[0];
                        writing = nums[1];
                        waiting = nums[2];
                    }
                }
            });

            if (active) document.getElementById('active').textContent = active;
            if (accepts) document.getElementById('accepted').textContent = accepts;
            if (handled) document.getElementById('handled').textContent = handled;
            if (requests) document.getElementById('requests').textContent = requests;
            if (reading) document.getElementById('reading').textContent = reading;
            if (writing) document.getElementById('writing').textContent = writing;
            if (waiting) document.getElementById('waiting').textContent = waiting;

            document.getElementById('time').textContent = new Date().toLocaleTimeString();
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

updateStatus();
setInterval(updateStatus, 3000);