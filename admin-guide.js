(() => {
  const search = document.getElementById('guideSearch');
  const sections = [...document.querySelectorAll('[data-guide-section]')];
  const empty = document.getElementById('guideNoResults');
  function filterGuide() {
    const query = search.value.trim().toLowerCase();
    let shown = 0;
    sections.forEach(section => {
      const matches = !query || section.textContent.toLowerCase().includes(query);
      section.hidden = !matches;
      if (matches) shown += 1;
    });
    empty.hidden = shown > 0;
  }
  search.addEventListener('input', filterGuide);
  document.addEventListener('keydown', event => {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); search.focus(); }
    if (event.key === 'Escape' && document.activeElement === search) { search.value = ''; filterGuide(); search.blur(); }
  });
  const sidebar = document.getElementById('sidebar'); const scrim = document.getElementById('sidebarScrim');
  document.getElementById('mobileMenu')?.addEventListener('click', () => { sidebar.classList.add('open'); scrim.classList.add('open'); });
  scrim?.addEventListener('click', () => { sidebar.classList.remove('open'); scrim.classList.remove('open'); });
})();
