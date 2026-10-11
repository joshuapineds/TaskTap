(() => {
  const toast = document.getElementById('toast');
  let toastTimer;
  function showToast(message) {
    toast.textContent = message;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 2800);
  }

  document.querySelectorAll('.future-link').forEach(link => {
    link.addEventListener('click', event => {
      event.preventDefault();
      showToast(`${link.dataset.page || 'This page'} is planned for a future admin milestone.`);
    });
  });

  document.querySelectorAll('[data-toast]').forEach(button => {
    button.addEventListener('click', event => {
      event.preventDefault();
      showToast(button.dataset.toast);
    });
  });

  document.getElementById('helpButton').addEventListener('click', () => { window.location.href = '[Admin] admin-guide.html#overview'; });
  document.getElementById('notificationButton').addEventListener('click', () => showToast('You have 8 open reports in this demo dashboard.'));
  document.getElementById('profileButton').addEventListener('click', () => showToast('Signed in as Admin Demo. Account controls are not connected.'));
  document.getElementById('adminProfile').addEventListener('click', () => showToast('Signed in as Admin Demo. Account controls are not connected.'));

  const refreshButton = document.getElementById('refreshButton');
  refreshButton.addEventListener('click', () => {
    refreshButton.disabled = true;
    refreshButton.innerHTML = '<span>↻</span> Updating…';
    setTimeout(() => {
      refreshButton.disabled = false;
      refreshButton.innerHTML = '<span>↻</span> Refresh';
      showToast('Demo dashboard refreshed. Sample figures are unchanged.');
    }, 650);
  });

  const periodData = {
    7: { posted: '286', completed: '214', note: 'Past 7 days' },
    30: { posted: '1,284', completed: '962', note: 'Past 30 days' },
    90: { posted: '3,642', completed: '2,718', note: 'Past 90 days' }
  };
  document.getElementById('periodSelect').addEventListener('change', event => {
    const data = periodData[event.target.value];
    document.getElementById('postedTotal').textContent = data.posted;
    document.getElementById('completedTotal').textContent = data.completed;
    document.getElementById('chartPeriodNote').textContent = data.note;
    showToast(`Showing sample activity for the ${event.target.options[event.target.selectedIndex].text.toLowerCase()}.`);
  });

  document.getElementById('exportButton').addEventListener('click', () => {
    const csv = [
      ['TaskTap Admin Dashboard — Demo Overview'],
      ['Metric', 'Sample value'],
      ['Total users', '2486'],
      ['Active tasks', '384'],
      ['Open reports', document.getElementById('queueCount').textContent],
      ['Recorded payments (PHP)', '184250'],
      ['Payment note', 'User-recorded amounts only; no transactions processed'],
      ['Tasks posted (last 30 days)', '1284'],
      ['Tasks completed (last 30 days)', '962']
    ].map(row => row.map(value => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\r\n');
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = 'tasktap-admin-demo-overview.csv';
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
    showToast('Demo overview exported as a CSV file.');
  });

  const extras = [
    ['Incomplete task details', 'Task · #TSK-1029', 'RP', 'Rico P.', 'Inaccurate listing', 'Today', 'Normal', 'task-type'],
    ['Repeated promotional posts', 'User · #USR-1984', 'LT', 'Liza T.', 'Spam', 'Yesterday', 'Low', 'user-type'],
    ['Task outside service area', 'Task · #TSK-1011', 'GM', 'Grace M.', 'Policy concern', 'Yesterday', 'Low', 'task-type'],
    ['Unresponsive task poster', 'User · #USR-1933', 'BC', 'Ben C.', 'User conduct', 'Oct 9', 'Low', 'user-type']
  ];
  const showQueue = document.getElementById('showQueue');
  let expanded = false;
  showQueue.addEventListener('click', () => {
    expanded = !expanded;
    if (expanded) {
      const body = document.getElementById('reportRows');
      extras.forEach(item => {
        const row = document.createElement('tr');
        row.dataset.report = '';
        const priorityClass = item[6] === 'Normal' ? 'normal' : 'low';
        row.innerHTML = `<td><div class="report-title"><span class="report-type ${item[7]}">${item[7] === 'task-type' ? 'T' : 'U'}</span><span><strong>${item[0]}</strong><small>${item[1]}</small></span></div></td><td><div class="person-cell"><span class="mini-avatar blue">${item[2]}</span><span>${item[3]}</span></div></td><td>${item[4]}</td><td>${item[5]}</td><td><span class="priority ${priorityClass}">${item[6]}</span></td><td><button class="row-more" aria-label="Report actions">···</button></td>`;
        body.appendChild(row);
      });
      document.querySelectorAll('.row-more').forEach(button => button.addEventListener('click', () => { window.location.href = '[Admin] report-management.html'; }));
      showQueue.innerHTML = 'Showing all 8 open reports <span>↑</span>';
    } else {
      document.querySelectorAll('#reportRows tr[data-report]').forEach((row, index) => { if (index > 3) row.remove(); });
      showQueue.innerHTML = 'Showing 4 of 8 open reports <span>→</span>';
    }
  });

  const menuButton = document.getElementById('mobileMenu');
  const sidebar = document.getElementById('sidebar');
  const scrim = document.getElementById('sidebarScrim');
  function closeSidebar() { sidebar.classList.remove('open'); scrim.classList.remove('open'); }
  menuButton.addEventListener('click', () => { sidebar.classList.toggle('open'); scrim.classList.toggle('open'); });
  scrim.addEventListener('click', closeSidebar);
  document.addEventListener('keydown', event => { if (event.key === 'Escape') closeSidebar(); });
})();
