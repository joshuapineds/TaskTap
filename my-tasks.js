(() => {
  const $ = s => document.querySelector(s);
  const $$ = s => [...document.querySelectorAll(s)];
  const toast = msg => {
    const el = $('#toast'); if (!el) return;
    el.textContent = msg; el.classList.add('show');
    clearTimeout(window.__myTasksToast);
    window.__myTasksToast = setTimeout(() => el.classList.remove('show'), 2300);
  };

  const cards = $$('.my-task');
  const tabs = $$('.tab');
  const noticeCopy = {
    posted: ['Posted tasks', 'These are tasks you created. Manage requests, edit details, or archive tasks when they are no longer active.'],
    requested: ['Requested tasks', 'Tasks where you have asked to help. Watch for the poster to accept or decline your request.'],
    accepted: ['Accepted tasks', 'Tasks where your request was accepted. Check the schedule and message the poster before starting.'],
    progress: ['Tasks in progress', 'Keep active work organized here. Use messages to confirm timing, scope, and completion details.'],
    completed: ['Completed tasks', 'Your finished task records and ratings are kept here for reference.']
  };
  const counts = {posted: 6, requested: 2, accepted: 1, progress: 1, completed: 8};
  let currentTab = 'posted';
  let query = '';
  let category = 'all';
  let sort = 'recent';

  const setNotice = tab => {
    const [title, body] = noticeCopy[tab];
    $('#tabNotice strong').textContent = title;
    $('#tabNotice p').textContent = body;
    $('#tabNotice a').textContent = tab === 'posted' ? 'Create another task →' : tab === 'requested' ? 'Find more tasks →' : 'View task activity →';
    $('#tabNotice a').href = tab === 'posted' || tab === 'requested' ? 'find-tasks.html' : 'dashboard.html';
  };

  const apply = () => {
    const filtered = cards.filter(card => {
      const matchesStatus = card.dataset.status === currentTab;
      const matchesCategory = category === 'all' || card.dataset.category === category;
      const matchesSearch = !query || card.dataset.search.includes(query);
      const show = matchesStatus && matchesCategory && matchesSearch;
      card.hidden = !show;
      return show;
    });

    const list = $('#taskList');
    const sorted = [...filtered].sort((a,b) => {
      if (sort === 'amount-high') return Number(b.dataset.amount) - Number(a.dataset.amount);
      if (sort === 'amount-low') return Number(a.dataset.amount) - Number(b.dataset.amount);
      if (sort === 'deadline') return new Date(a.dataset.date) - new Date(b.dataset.date);
      return cards.indexOf(a) - cards.indexOf(b);
    });
    sorted.forEach(card => list.appendChild(card));

    $('#resultCount').textContent = `${filtered.length} task${filtered.length === 1 ? '' : 's'}`;
    $('#resultLabel').textContent = `in ${noticeCopy[currentTab][0]}`;
    $('#emptyState').hidden = filtered.length !== 0;
  };

  tabs.forEach(tab => tab.addEventListener('click', () => {
    currentTab = tab.dataset.tab;
    tabs.forEach(t => { const active = t === tab; t.classList.toggle('active', active); t.setAttribute('aria-selected', active ? 'true' : 'false'); });
    setNotice(currentTab); apply();
  }));

  $('#taskSearch')?.addEventListener('input', e => { query = e.target.value.trim().toLowerCase(); apply(); });
  $('#categoryFilter')?.addEventListener('change', e => { category = e.target.value; apply(); });
  $('#sortTasks')?.addEventListener('change', e => { sort = e.target.value; apply(); });

  $('#clearFilters')?.addEventListener('click', () => {
    $('#taskSearch').value = ''; $('#categoryFilter').value = 'all'; $('#sortTasks').value = 'recent';
    query = ''; category = 'all'; sort = 'recent'; apply();
  });

  $('#listView')?.addEventListener('click', () => {
    $('#taskList').classList.remove('compact-view'); $('#listView').classList.add('active'); $('#compactView').classList.remove('active');
  });
  $('#compactView')?.addEventListener('click', () => {
    $('#taskList').classList.add('compact-view'); $('#compactView').classList.add('active'); $('#listView').classList.remove('active');
  });

  $$('.save-task').forEach(btn => btn.addEventListener('click', () => {
    const saved = btn.classList.toggle('saved');
    btn.textContent = saved ? '♥' : '♡';
    btn.setAttribute('aria-label', saved ? 'Unsave task' : 'Save task');
    toast(saved ? 'Task saved to your favorites.' : 'Task removed from your favorites.');
  }));

  $$('.text-action, .my-task .btn:not(a)').forEach(btn => btn.addEventListener('click', e => {
    if (btn.closest('.save-task')) return;
    if (btn.dataset.href) { e.preventDefault(); location.href = btn.dataset.href; return; }
    if (btn.dataset.message) toast(btn.dataset.message);
  }));

  $('#topSearch')?.addEventListener('keydown', e => {
    if (e.key === 'Enter') {
      e.preventDefault();
      const value = e.target.value.trim();
      if (value) { $('#taskSearch').value = value; query = value.toLowerCase(); apply(); $('#taskSearch').focus(); }
    }
  });

  // Allow Edit Task to read the same sample data used across the prototype.
  const params = new URLSearchParams(location.search);
  if (params.get('tab') && counts[params.get('tab')]) {
    const tab = tabs.find(t => t.dataset.tab === params.get('tab'));
    tab?.click();
  }

  apply();
})();
