n=0
until [ "$n" -ge 3 ]
do
   $@
   if [ $? -eq 0 ]; then
     break;
   fi 
   n=$((n+1)) 
   sleep 5
   if [ "$n" -ge 3 ]; then
     echo "All retries failed"
     exit 1
   fi
   echo "retrying command"
done
