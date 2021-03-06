(use dotnet)

(clr-reference 'System.Windows.Forms)
(clr-using 'System.Windows.Forms)

(define form (clr-new 'Form))

(define btn (clr-new 'Button))
(clr-prop-set! btn 'Text "Click Me!")
(letrec ([click (lambda (sender e) 
                  (print (clr->string (clr-prop-get sender 'Text)))
                  (print e)
                  (clr-call 'Show 'MessageBox "Hello Gauche DotNet World!!")
                  (clr-event-remove! btn 'Click click)
                  )])
  (clr-event-add! btn 'Click click))

(clr-call 'Add (clr-prop-get form 'Controls) btn)

(clr-call 'Run 'Application form)

(print "finish")

