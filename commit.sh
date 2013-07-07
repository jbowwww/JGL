datestr=`date +%y%m%d-%H%M`
archive=../Backups/JGL-$datestr.tar.gz
commitMsg="$*"

echo Archiving locally to $archive
echo "$commitMsg" > commitMsg.txt
tar -cz --exclude=./\.* -f ../Backups/$archive .
rm -f commitMsg.txt

echo Updating index with working directory contents
git add -A .

echo Changes:
git status

echo Committing...
git commit -m "$*"

git push -v --all

echo Done.
