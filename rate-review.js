(() => {
  const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
  const form = $('#reviewForm'), rating = $('#rating'), ratingText = $('#ratingText'), ratingHint = $('#ratingHint');
  const labels = {1:['Poor','The experience needs significant improvement.'],2:['Fair','There were several issues to work through.'],3:['Good','A solid experience with room for improvement.'],4:['Very good','A positive experience overall.'],5:['Excellent','A reliable, respectful experience worth recommending.']};
  const stars = $$('#starPicker button');
  function setRating(n){ rating.value = n; stars.forEach(s => s.classList.toggle('active', +s.dataset.rating <= n)); ratingText.textContent = labels[n][0]; ratingHint.textContent = labels[n][1]; }
  stars.forEach(s => { s.addEventListener('mouseenter',()=>stars.forEach(x=>x.classList.toggle('active',+x.dataset.rating<=+s.dataset.rating))); s.addEventListener('mouseleave',()=>setRating(+rating.value||0)); s.addEventListener('click',()=>setRating(+s.dataset.rating)); });
  const title=$('#reviewTitle'), text=$('#reviewText');
  function counter(input,el){el.textContent=`${input.value.length} / ${input.maxLength}`}
  title.addEventListener('input',()=>counter(title,$('#titleCount'))); text.addEventListener('input',()=>counter(text,$('#reviewCount')));
  $$('.quick-tags button').forEach(btn=>btn.addEventListener('click',()=>{ const phrase=btn.dataset.tag; const current=text.value.trim(); text.value=current ? `${current}${current.endsWith('.')?'':' ·'} ${phrase}.` : `${phrase}.`; counter(text,$('#reviewCount')); text.focus(); }));
  function clearErrors(){ ['titleError','reviewError','confirmError'].forEach(id=>$('#'+id).textContent=''); }
  form.addEventListener('submit',e=>{ e.preventDefault(); clearErrors(); let ok=true; if(!rating.value){ratingHint.textContent='Please choose a rating.';ratingHint.style.color='#c64f42';ok=false;} else ratingHint.style.color=''; if(!title.value.trim()){ $('#titleError').textContent='Add a short review title.';ok=false;} if(!text.value.trim()||text.value.trim().length<10){$('#reviewError').textContent='Write at least 10 characters so the review is useful.';ok=false;} if(!$('#confirmReview').checked){$('#confirmError').textContent='Please confirm that your review is honest and does not contain sensitive information.';ok=false;} if(!ok)return; $('#publishedStars').textContent='★★★★★'.slice(0,+rating.value)+'☆☆☆☆☆'.slice(0,5-+rating.value); $('#publishedTitle').textContent=title.value.trim(); $('#publishedText').textContent=text.value.trim(); $('#successModal').classList.add('open'); document.body.classList.add('modal-open'); });
  $('#successClose').addEventListener('click',()=>{$('#successModal').classList.remove('open');document.body.classList.remove('modal-open')}); $('#successModal').addEventListener('click',e=>{if(e.target.id==='successModal'){$('#successModal').classList.remove('open');document.body.classList.remove('modal-open')}});
  counter(title,$('#titleCount'));counter(text,$('#reviewCount'));
  document.addEventListener('keydown',e=>{if(e.key==='Escape'){$('#successModal').classList.remove('open');document.body.classList.remove('modal-open')}});
})();
